using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UI.TitleScreen.Backup;
using UnityEngine;
using World.Serialization;
using WorldModule;

namespace World.BackUp
{
    public static class WorldBackUpUtils
    {
        private const string DATE_FORMAT = "yyyy_MM_dd";
        private const string TIME_FORMAT = "HH_mm_ss";
        public static void BackUpWorld(string worldName)
        {
            string backupFolderPath = WorldLoadUtils.GetBackUpPath(worldName);
            DateTime currentTime = DateTime.Now;
            string formattedDate = FormatDate(currentTime);
            string backupDateFolderPath = Path.Combine(backupFolderPath, formattedDate);
            if (!Directory.Exists(backupDateFolderPath))
            {
                Directory.CreateDirectory(backupDateFolderPath);
            }

            string formattedTime = FormatTime(currentTime);
            string backupFilePathBackup = Path.Combine(backupDateFolderPath, formattedTime);
            if (Directory.Exists(backupFilePathBackup))
            {
                Debug.LogWarning($"Backup already exists at path {backupFilePathBackup}");
                return;
            }

            RemoveRecentFolderBackups(backupDateFolderPath);
            string mainPath = WorldLoadUtils.GetMainPath(worldName);
            Debug.Log($"Created backup of world '{worldName}' at path '{backupFilePathBackup}'");
            Directory.CreateDirectory(backupFilePathBackup);
            CopyDirectory(mainPath, backupFilePathBackup);
        }

        public static void RestoreBackUp(string worldName, string backupPath)
        {
            if (!Directory.Exists(backupPath))
            {
                Debug.LogError($"Backup doesn't exist");
                return;
            }
            string mainPath = WorldLoadUtils.GetMainPath(worldName);
            Debug.Log($"Restored world '{worldName}' from path {backupPath}");
            CopyDirectory(backupPath, mainPath);
        }

        private static string FormatDate(DateTime dateTime)
        {
            return dateTime.ToString(DATE_FORMAT);
        }
        
        private static string FormatTime(DateTime dateTime)
        {
            return dateTime.ToString(TIME_FORMAT);
        }

        public static void CleanUpBackups(string worldName)
        {
            string backupFolderPath = WorldLoadUtils.GetBackUpPath(worldName);
            string[] dateFolders = Directory.GetDirectories(backupFolderPath);
            Array.Sort(dateFolders);
            Array.Reverse(dateFolders);
            const int CLEAR_START_INDEX = 2;

            int removed = 0;
            // Save the most recent one per hour
            for (int i = 0; i < CLEAR_START_INDEX; i++)
            {
                removed += RemoveRecentFolderBackups(dateFolders[i]);
            }

            // Delete all but the most recent backup within the folder
            for (int i = CLEAR_START_INDEX; i < dateFolders.Length; i++)
            {
                removed += RemoveOldFolderBackups(dateFolders[i]);   
            }
            Debug.Log($"Removed {removed} old backups");
        }

        private static int RemoveRecentFolderBackups(string dateFolder)
        {
            int removed = 0;
            Dictionary<int, (string, DateTime)> mostRecentHourDates = new Dictionary<int, (string,DateTime)>();
            HashSet<string> pathsToSave = new HashSet<string>();
            string[] backupPaths = Directory.GetDirectories(dateFolder);
            Array.Sort(backupPaths);
            Array.Reverse(backupPaths);
            const int CURRENT_START_INDEX = 10;
            for (int n = 0; n < Mathf.Min(CURRENT_START_INDEX,backupPaths.Length); n++) // Save 10 most recent backups
            {
                string backUpPath = backupPaths[n];
                pathsToSave.Add(backUpPath);
            }

            for (int n = CURRENT_START_INDEX; n < backupPaths.Length; n++)
            {
                string backUpPath = backupPaths[n];
                string timeString = Path.GetFileName(backUpPath);
                bool timeFormatValid = DateTime.TryParseExact(timeString, TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var backupTime);
                if (!timeFormatValid) // Don't delete invalid folders cause user probably manually changed them
                {
                    pathsToSave.Add(backUpPath); // This will save paths of the current hour
                    continue;
                }
                int hour = backupTime.TimeOfDay.Hours;
                if (!mostRecentHourDates.TryGetValue(hour, out var date))
                {
                    mostRecentHourDates[hour] = (backUpPath, backupTime);
                    pathsToSave.Add(backUpPath);
                    continue;
                }

                var (mostRecentPath, mostRecentDate) = date;
                if (backupTime.TimeOfDay <= mostRecentDate.TimeOfDay) continue;
                pathsToSave.Add(backUpPath);
                pathsToSave.Remove(mostRecentPath);
                mostRecentHourDates[hour] = (backUpPath, backupTime);
            }
            
            foreach (string backupPath in backupPaths)
            {
                if (pathsToSave.Contains(backupPath)) continue;
                Directory.Delete(backupPath, true);
                removed++;
            }

            return removed;
        }

        private static int RemoveOldFolderBackups(string dateFolder)
        {
            int removed = 0;
            string[] backupPaths = Directory.GetDirectories(dateFolder);
            Array.Sort(backupPaths);
            Array.Reverse(backupPaths);
                
            for (int n = 1; n < backupPaths.Length; n++)
            {
                string backUpPath = backupPaths[n];
                string timeString = Path.GetFileName(backUpPath);
                bool timeFormatValid = DateTime.TryParseExact(timeString, TIME_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out var backupTime);
                if (!timeFormatValid) continue; // Don't delete invalid folders
                Directory.Delete(backUpPath,true);
                removed++;
            }

            return removed;
        }
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceDir);
            
            if (!sourceDirInfo.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            }
            
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            foreach (FileInfo file in sourceDirInfo.GetFiles())
            {
                string targetFilePath = Path.Combine(targetDir, file.Name);
                file.CopyTo(targetFilePath, overwrite: true);
            }
            
            foreach (DirectoryInfo subDir in sourceDirInfo.GetDirectories())
            {
                string newTargetDir = Path.Combine(targetDir, subDir.Name);
                CopyDirectory(subDir.FullName, newTargetDir);
            }
        }
    }
}

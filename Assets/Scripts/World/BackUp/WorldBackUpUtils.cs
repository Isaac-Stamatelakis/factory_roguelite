using System;
using System.IO;
using UnityEngine;
using World.Serialization;
using WorldModule;

namespace World.BackUp
{
    public static class WorldBackUpUtils
    {
        public static void BackUpWorld(string worldName)
        {
            string backupFolderPath = WorldLoadUtils.GetBackUpPath(worldName);
            DateTime currentTime = DateTime.Now;
            string formattedDate = currentTime.ToString("yyyy_MM_dd");
            string backupFilePath = Path.Combine(backupFolderPath, formattedDate);
            if (!Directory.Exists(backupFilePath))
            {
                Directory.CreateDirectory(backupFilePath);
            }
            string formattedTime = currentTime.ToString("HH_mm_ss");
            string backupFilePathBackup = Path.Combine(backupFilePath, formattedTime);
            if (Directory.Exists(backupFilePathBackup))
            {
                Debug.LogWarning($"Backup already exists at path {backupFilePathBackup}");
                return;
            }
            string mainPath = WorldLoadUtils.GetMainPath(worldName);
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
            Debug.Log($"Restored world from path {backupPath} into {mainPath}");
            CopyDirectory(backupPath, mainPath);
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

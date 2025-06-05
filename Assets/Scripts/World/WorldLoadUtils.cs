using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;
using DevTools;
using Newtonsoft.Json;
using UI.QuestBook;
using World.Serialization;


namespace WorldModule {
    public enum WorldFileType
    {
        DimensionFolder,
        Player,
        GameStage
    }
    
    
    public static class WorldLoadUtils
    {
        // Change this for accessing worlds in other folders persisent.
        public static bool UsePersistentPath = true;
        
        public const string DEFAULT_WORLD_FOLDER = "worlds"; 
        public const string PLAYER_DATA_FILE = "player_data.bin";
        public const string GAME_STAGE_FILE = "stage_data.bin";
        public const string DIMENSION_FOLDER_PATH = "Dimensions";
        public const string DIM_FOLDER_PREFIX = "dim";
        public const string META_DATA_PATH = "meta.bin";
        public const string BACKUP_FOLDER_PATH = "Backups";
        public const string CURRENT_FOLDER_PATH = "Main";
        
        public static string GetWorldComponentPath(WorldFileType worldFileType)
        {
            return GetWorldComponentPath(WorldManager.GetInstance().GetWorldName(), worldFileType);
        }
        
        public static string GetWorldComponentPath(string worldName, WorldFileType worldFileType)
        {
            string value = GetWorldFileName(worldFileType);
            string worldPath = GetWorldPath(worldName);
            if (!UsePersistentPath) // Loads directly without backups
            {
                return Path.Combine(worldPath, value);
            }
            string mainFolderPath = Path.Combine(worldPath, CURRENT_FOLDER_PATH);
            return Path.Combine(mainFolderPath, value);
        }

        public static string GetBackUpPath(string worldName)
        {
            return Path.Combine(GetWorldPath(worldName), BACKUP_FOLDER_PATH);
        }
        
        public static string GetMainPath(string worldName)
        {
            return Path.Combine(GetWorldPath(worldName), CURRENT_FOLDER_PATH);
        }
        

        private static string GetWorldFileName(WorldFileType worldFileType)
        {
            return worldFileType switch
            {
                WorldFileType.DimensionFolder => DIMENSION_FOLDER_PATH,
                WorldFileType.Player => PLAYER_DATA_FILE,
                WorldFileType.GameStage => GAME_STAGE_FILE,
                _ => throw new ArgumentOutOfRangeException(nameof(worldFileType), worldFileType, null)
            };
        }

        public static string GetWorldFileJson(WorldFileType worldFileType)
        {
            string path = GetWorldComponentPath(worldFileType);
            return GetWorldFileJson(path);
        }

        public static void SaveWorldFileJson(WorldFileType worldFileType, string json)
        {
            string path = GetWorldComponentPath(worldFileType);
            byte[] compressed = CompressString(json);
            File.WriteAllBytes(path,compressed);
        }
        
        public static string GetWorldFileJson(string worldName, WorldFileType worldFileType)
        {
            string path = GetWorldComponentPath(worldName, worldFileType);
            return GetWorldFileJson(path);
        }

        private static string GetWorldFileJson(string path)
        {
            byte[] compressed = File.ReadAllBytes(path);
            string json = DecompressString(compressed);
            return json;
        }
        
        public static byte[] CompressString(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (var outputStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    gzipStream.Write(inputBytes, 0, inputBytes.Length);
                }
                return outputStream.ToArray();
            }
        }
        
        public static string DecompressString(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var outputStream = new MemoryStream())
            {
                gzipStream.CopyTo(outputStream);
                byte[] outputBytes = outputStream.ToArray();
                return Encoding.UTF8.GetString(outputBytes);
            }
        }

        public static string GetMetaDataPath(string worldName)
        {
            string worldPath = GetWorldPath(worldName);
            return Path.Combine(worldPath, META_DATA_PATH);
        }

        public static WorldMetaData GetWorldMetaData(string worldName)
        {
            string metaDataPath = GetMetaDataPath(worldName);
            if (!File.Exists(metaDataPath))
            {
                if (META_DATA_PATH.EndsWith(".json"))
                {
                    string oldPath = metaDataPath;
                    metaDataPath = metaDataPath.Replace(".json", ".bin");
                    File.Delete(oldPath);
                }
                WorldCreation.InitializeMetaData(metaDataPath,new WorldCreationData(worldName,null,false,QuestBookUtils.MAIN_QUEST_BOOK_NAME));
            }

            byte[] compressed = File.ReadAllBytes(metaDataPath);
            string json = DecompressString(compressed);
            WorldMetaData worldMetaData = JsonConvert.DeserializeObject<WorldMetaData>(json);
            return worldMetaData;
        }

        public static void WriteMetaData(string worldName, WorldMetaData worldMetaData)
        {
            string worldPath = GetWorldPath(worldName);
            string path = Path.Combine(worldPath, META_DATA_PATH);
            byte[] bytes = CompressString(JsonConvert.SerializeObject(worldMetaData));
            File.WriteAllBytes(path, bytes);
        }
        
        
        public static string GetDimPath(int dim) {
            return Path.Combine(GetWorldComponentPath(WorldFileType.DimensionFolder),DIM_FOLDER_PREFIX + dim.ToString());
        }
        public static string GetDimPath(string worldName, int dim) {
            return Path.Combine(worldName, GetWorldFileName( WorldFileType.DimensionFolder),DIM_FOLDER_PREFIX + dim.ToString());
        }
        public static bool DimExists(int dim) {
            string path = GetDimPath(dim);
            return pathExists(path);
        }

        public static string GetWorldPath(string worldName)
        {
            if (UsePersistentPath)
            {
                return Path.Combine(Application.persistentDataPath, DEFAULT_WORLD_FOLDER, worldName);
            }

            return worldName;

        }

        public static string GetCurrentWorldPath()
        {
            return GetWorldPath(WorldManager.GetInstance().GetWorldName());
        }
        public static bool dimExists(string worldName, int dim) {
            string path = GetDimPath(dim);
            return pathExists(path);
        }
        public static void createDimFolder(int dim) {
            string path = GetDimPath(dim);
            Directory.CreateDirectory(path);
        }
        public static bool worldExists(string path) {
            string worldPath = Path.Combine(Application.persistentDataPath,path);
            return pathExists(worldPath);
        }
        public static bool defaultWorldExists(string worldName) {
            return pathExists(GetWorldPath(worldName));
        }
        public static bool pathExists(string path) {
            return Directory.Exists(path) || File.Exists(path);
        }
        
        public static void InitializeQuestBook(string worldName)
        {
            string mainLibPath = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), QuestBookUtils.MAIN_QUEST_BOOK_NAME);
            string playerQuestBookPath = Path.Combine(WorldLoadUtils.GetMainPath(worldName), QuestBookUtils.WORLD_QUEST_FOLDER_PATH);
            QuestBookUtils.VerifyIntegrityOfQuestBookData(mainLibPath,playerQuestBookPath);
        }
    }
}


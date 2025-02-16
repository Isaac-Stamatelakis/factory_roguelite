using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using World.Serialization;


namespace WorldModule {
    public enum WorldFileType
    {
        Meta,
        DimensionFolder,
        Player,
        Questbook,
        StructureMeta
    }
    
    
    public static class WorldLoadUtils
    {
        // Change this for accessing worlds in other folders persisent.
        public static bool UsePersistentPath = true;
        
        private const string defaultWorldFolder = "worlds"; 
        private const string playerDataName = "player_data.json";
        private const string dimensionFolderName = "Dimensions";
        private const string dimFolderPrefix = "dim";
        private const string META_DATA_PATH = "meta.json";
        private const string QUESTBOOK_PATH = "questbook.json";
        private const string STRUCTURE_META_PATH = "structure_meta.json";

        public static string DefaultWorldFolder => defaultWorldFolder;
        public static string GetWorldComponentPath(WorldFileType worldFileType)
        {
            return GetWorldComponentPath(WorldManager.getInstance().GetWorldName(), worldFileType);
        }
        
        public static string GetWorldComponentPath(string worldName, WorldFileType worldFileType)
        {
            string value = GetWorldFileValue(worldFileType);
            string worldPath = GetWorldPath(worldName);
            return Path.Combine(worldPath, value);
        }

        private static string GetWorldFileValue(WorldFileType worldFileType)
        {
            return worldFileType switch
            {
                WorldFileType.Meta => META_DATA_PATH,
                WorldFileType.DimensionFolder => dimensionFolderName,
                WorldFileType.Player => playerDataName,
                WorldFileType.Questbook => QUESTBOOK_PATH,
                WorldFileType.StructureMeta => STRUCTURE_META_PATH,
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

        public static WorldMetaData GetWorldMetaData(string worldName)
        {
            string worldPath = GetWorldPath(worldName);
            string metaDataPath = Path.Combine(worldPath, GetWorldFileValue(WorldFileType.Meta));
            if (!File.Exists(metaDataPath))
            {
                WorldCreation.InitializeMetaData(metaDataPath);
            }

            byte[] compressed = File.ReadAllBytes(metaDataPath);
            string json = DecompressString(compressed);
            WorldMetaData worldMetaData = JsonConvert.DeserializeObject<WorldMetaData>(json);
            return worldMetaData;
        }
        
        
        public static string GetDimPath(int dim) {
            return Path.Combine(GetWorldComponentPath(WorldFileType.DimensionFolder),dimFolderPrefix + dim.ToString());
        }
        public static string GetDimPath(string worldName, int dim) {
            return Path.Combine(worldName, GetWorldFileValue( WorldFileType.DimensionFolder),dimFolderPrefix + dim.ToString());
        }
        public static bool DimExists(int dim) {
            string path = GetDimPath(dim);
            return pathExists(path);
        }

        public static string GetWorldPath(string worldName)
        {
            if (UsePersistentPath)
            {
                return Path.Combine(Application.persistentDataPath, defaultWorldFolder, worldName);
            }

            return worldName;

        }

        public static string GetCurrentWorldPath()
        {
            return GetWorldPath(WorldManager.getInstance().GetWorldName());
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
    }
}


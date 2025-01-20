using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
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
        public static bool UsePersistentPath = true;
        
        private static readonly string defaultWorldFolder = "worlds"; 
        private static readonly string playerDataName = "player_data.json";
        private static readonly string dimensionFolderName = "Dimensions";
        private static readonly string dimFolderPrefix = "dim";
        private static readonly string META_DATA_PATH = "meta.json";
        private static readonly string QUESTBOOK_PATH = "questbook.json";
        private static readonly string STRUCTURE_META_PATH = "structure_meta.json";

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

        public static WorldMetaData GetWorldMetaData(string worldName)
        {
            string worldPath = GetWorldPath(worldName);
            string metaDataPath = Path.Combine(worldPath, GetWorldFileValue(WorldFileType.Meta));
            if (!File.Exists(metaDataPath))
            {
                WorldCreation.InitializeMetaData(metaDataPath);
            }
            WorldMetaData worldMetaData = JsonConvert.DeserializeObject<WorldMetaData>(File.ReadAllText(metaDataPath));
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


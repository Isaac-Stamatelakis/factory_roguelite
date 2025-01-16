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
        Questbook
    }
    
    
    public static class WorldLoadUtils
    {
        private static readonly string defaultWorldFolder = "worlds"; 
        private static readonly string playerDataName = "player_data.json";
        private static readonly string dimensionFolderName = "Dimensions";
        private static readonly string dimFolderPrefix = "dim";
        private static readonly string META_DATA_PATH = "meta.json";
        private static readonly string QUESTBOOK_PATH = "questbook.json";

        public static string DefaultWorldFolder => defaultWorldFolder;

        public static string PlayerDataName => playerDataName;

        public static string DimensionFolderName => dimensionFolderName;

        public static string getFullWorldPath() {
            return getFullWorldPath(WorldManager.getInstance().getWorldPath());
        }
        public static string GetWorldFilePath(WorldFileType worldFileType)
        {
            string value = GetWorldFileValue(worldFileType);
            string worldPath = getFullWorldPath();
            return Path.Combine(worldPath, value);
        }

        private static string GetWorldFileValue(WorldFileType worldFileType)
        {
            switch (worldFileType)
            {
                case WorldFileType.Meta:
                    return META_DATA_PATH;
                case WorldFileType.DimensionFolder:
                    return DimensionFolderName;
                case WorldFileType.Player:
                    return playerDataName;
                case WorldFileType.Questbook:
                    return QUESTBOOK_PATH;
                default:
                    throw new ArgumentOutOfRangeException(nameof(worldFileType), worldFileType, null);
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
            WorldMetaData worldMetaData = JsonConvert.DeserializeObject<WorldMetaData>(File.ReadAllText(metaDataPath));
            return worldMetaData;
        }
        public static string getFullWorldPath(string path) {
            return Path.Combine(Application.persistentDataPath,path);
        }
        public static string getPlayerDataPath() {
            return getPlayerDataPath(WorldManager.getInstance().getWorldPath());
        }
        public static string getPlayerDataPath(string worldName) {
            string worldPath = getFullWorldPath(worldName);
            return Path.Combine(worldPath,playerDataName);
        }
        public static string getDimensionFolderPath() {
            return Path.Combine(getFullWorldPath(),dimensionFolderName);
        }
        public static string getDimensionFolderPath(string worldName) {
            return Path.Combine(getFullWorldPath(worldName),dimensionFolderName);
        }
        public static string getDimPath(int dim) {
            return Path.Combine(getDimensionFolderPath(),dimFolderPrefix + dim.ToString());
        }
        public static string getDimPath(string worldName, int dim) {
            return Path.Combine(getDimensionFolderPath(worldName),dimFolderPrefix + dim.ToString());
        }
        public static bool dimExists(int dim) {
            string path = getDimPath(dim);
            return pathExists(path);
        }

        public static string GetWorldPath(string worldName)
        {
            string worldFolder = Path.Combine(Application.persistentDataPath,defaultWorldFolder);
            return Path.Combine(worldFolder, worldName);
        }
        public static bool dimExists(string worldName, int dim) {
            string path = getDimPath(dim);
            return pathExists(path);
        }
        public static void createDimFolder(int dim) {
            string path = getDimPath(dim);
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


namespace WorldModule {
    public static class WorldLoadUtils
    {
        private readonly static string defaultWorldFolder = "worlds"; 
        private readonly static string playerDataName = "player_data.json";
        private readonly static string dimensionFolderName = "Dimensions";
        private readonly static string dimFolderPrefix = "dim";

        public static string DefaultWorldFolder => defaultWorldFolder;

        public static string PlayerDataName => playerDataName;

        public static string DimensionFolderName => dimensionFolderName;

        public static string getFullWorldPath() {
            return getFullWorldPath(WorldManager.getInstance().getWorldPath());
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
            string worldPath = getFullWorldPath(Path.Combine(DefaultWorldFolder,worldName));
            return pathExists(worldPath);
        }
        public static bool pathExists(string path) {
            return Directory.Exists(path) || File.Exists(path);
        }
    }
}


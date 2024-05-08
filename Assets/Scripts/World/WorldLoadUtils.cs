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

        public static string DefaultWorldFolder => defaultWorldFolder;

        public static string PlayerDataName => playerDataName;

        public static string DimensionFolderName => dimensionFolderName;

        public static string getWorldPath() {
            return Path.Combine(Application.persistentDataPath,WorldManager.getInstance().getWorldPath());
        }
        public static string getPathOfDefaultWorld(string worldName) {
            string folderPath = Path.Combine(Application.persistentDataPath,defaultWorldFolder);
            return Path.Combine(folderPath,worldName);
        }
        public static string getPlayerDataPath() {
            return Path.Combine(getWorldPath(),playerDataName);
        }
        public static string getDimensionFolderPath() {
            return Path.Combine(getWorldPath(),dimensionFolderName);
        }
        public static string getDimPath(int dim) {
            return Path.Combine(getDimensionFolderPath(),"dim" + dim.ToString());
        }
        public static bool dimExists(int dim) {
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
            string worldPath = getPathOfDefaultWorld(worldName);
            return pathExists(worldPath);
        }
        public static bool pathExists(string path) {
            return Directory.Exists(path) || File.Exists(path);
        }
    }
}


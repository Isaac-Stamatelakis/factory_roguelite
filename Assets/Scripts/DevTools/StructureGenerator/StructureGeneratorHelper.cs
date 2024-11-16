using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule.Caves;
using System.IO;
using WorldModule;
using PlayerModule;
using PlayerModule.IO;
using RobotModule;
using Chunks;
using Chunks.IO;
using Chunks.Partitions;
using System.Linq;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR 
using DevTools;
#endif

namespace DevTools.Structures {
    public enum StructureGenOptionType {
        Empty,
        Fill,
        Border
    }
    public static class StructureGeneratorHelper
    {
        private static string parameterId = "structure_parameter";
        private static string fillId = "structure_fill";
        private static string folder = "structureDev";
        
        public static string ParimeterId { get => parameterId; set => parameterId = value; }
        public static string FillId { get => fillId; set => fillId = value; }
        public static string getPath(string structureName) {
            string folderPath = getFolderPath();
            return Path.Combine(folderPath,structureName);
        }
        public static string getFolderPath() {
            string folderPath = Path.Combine(DevToolUtils.getDevPath(),folder);
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }

        public static string[] getAllStructureFolders() {
            string folderPath = getFolderPath();
            string[] fullPaths = Directory.GetDirectories(folderPath);

            string[] directoryNames = new string[fullPaths.Length];
            for (int i = 0; i < fullPaths.Length; i++) {
                directoryNames[i] = Path.GetFileName(fullPaths[i]);
            }
            return directoryNames;
        }
      
        public static void newStructure(string name, StructureGenerationOption generationOption, IntervalVector bounds) {
            string folderPath = getFolderPath();
            string path = Path.Combine(folderPath,name);
            Directory.CreateDirectory(path);
            WorldManager.getInstance().setWorldPath(path);
            Debug.Log("Structure World Created at " + path);
            string dimensionPath = WorldLoadUtils.getDimensionFolderPath();
            Directory.CreateDirectory(dimensionPath);
            Debug.Log("Dimension Folder Created at " + dimensionPath);
            string structureDimPath = WorldLoadUtils.getDimPath(0);
            Directory.CreateDirectory(structureDimPath);
            Vector2Int caveSize = bounds.getSize();
            WorldTileConduitData dimData = WorldCreation.CreateEmptyWorldData(bounds);
            if (generationOption != null) {
                generationOption.apply(dimData);
            }
            WorldGenerationFactory.SaveToJson(dimData,caveSize,0,structureDimPath);


            List<SerializedItemSlot> inventory = new List<SerializedItemSlot>();
            inventory.Add(new SerializedItemSlot(
                parameterId,
                999,
                null
            ));
            inventory.Add(new SerializedItemSlot(
                fillId,
                999,
                null
            ));
            for (int i = 0; i < 38; i++) {
               inventory.Add(new SerializedItemSlot(null,0,null)); 
            }
            string seralizedInv = Newtonsoft.Json.JsonConvert.SerializeObject(inventory);
            PlayerData playerData = new PlayerData(
                x: 0,
                y: 0,
                playerRobot: RobotDataFactory.getDefaultRobotString(true),
                name: "Izakio",
                inventoryJson: seralizedInv
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            string playerDataPath = Path.Combine(path,"player_data.json");
            File.WriteAllText(playerDataPath,json);
        }
    }
}

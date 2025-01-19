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
using Item.Slot;
using Items;
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
        
        public static string ParimeterId { get => parameterId; set => parameterId = value; }
        public static string FillId { get => fillId; set => fillId = value; }
        public static string getPath(string structureName) {
            string folderPath = GetFolderPath();
            return Path.Combine(folderPath,structureName);
        }
        public static string GetFolderPath()
        {
            string folderPath = DevToolUtils.GetDevToolPath(DevTool.Structure);
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath;
        }

        public static string[] GetAllStructureFolders() {
            string folderPath = GetFolderPath();
            string[] fullPaths = Directory.GetDirectories(folderPath);

            string[] directoryNames = new string[fullPaths.Length];
            for (int i = 0; i < fullPaths.Length; i++) {
                directoryNames[i] = Path.GetFileName(fullPaths[i]);
            }
            return directoryNames;
        }
      
        public static void NewStructure(string name, StructureGenerationOption generationOption, IntervalVector bounds) {
            string folderPath = GetFolderPath();
            string path = Path.Combine(folderPath,name);
            Directory.CreateDirectory(path);
            
            WorldLoadUtils.UsePersistentPath = false;
            WorldManager.getInstance().SetWorldName(path);
            Debug.Log("Structure World Created at " + path);
            
            string dimensionPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.DimensionFolder);
            Directory.CreateDirectory(dimensionPath);
            Debug.Log("Dimension Folder Created at " + dimensionPath);
            string structureDimPath = WorldLoadUtils.GetDimPath(0);
            Directory.CreateDirectory(structureDimPath);
            Vector2Int caveSize = bounds.getSize();
            WorldTileConduitData dimData = WorldCreation.CreateEmptyWorldData(bounds);
            if (generationOption != null) {
                generationOption.apply(dimData);
            }
            WorldGenerationFactory.SaveToJson(dimData,caveSize,0,structureDimPath);
            PlayerInventoryData playerInventoryData = PlayerInventoryFactory.GetDefault();
            playerInventoryData.Inventory[0] = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(parameterId),
                Global.MaxSize, null);
            playerInventoryData.Inventory[0] = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(parameterId),
                Global.MaxSize, null);
            
            PlayerData playerData = new PlayerData(
                x: 0,
                y: 0,
                playerRobot: RobotDataFactory.GetDefaultRobotData(),
                name: "Izakio",
                sInventoryData: PlayerInventoryFactory.Serialize(playerInventoryData)
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            string playerDataPath = Path.Combine(path,"player_data.json");
            File.WriteAllText(playerDataPath,json);
        }
    }
}

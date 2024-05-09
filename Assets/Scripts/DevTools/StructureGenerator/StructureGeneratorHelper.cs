using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldModule.Caves;
using System.IO;
using WorldModule;
using PlayerModule;
using PlayerModule.IO;
using RobotModule;
using ChunkModule;
using ChunkModule.IO;

namespace DevTools.Structures {
    public static class StructureGeneratorHelper
    {
        private static string parameterId = "structure_parameter";
        private static string fillId = "structure_fill";
        private static string folder = "structureDev";

        public static string ParameterId { get => parameterId; set => parameterId = value; }
        public static string FillId { get => fillId; set => fillId = value; }
        private static IntervalVector structDimBounds = new IntervalVector(new Interval<int>(-4,4), new Interval<int>(-4,4));
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
      
        public static void newStructure(string name) {
            
            string folderPath = getFolderPath();
            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }
            string path = Path.Combine(folderPath,name);
            Directory.CreateDirectory(path);
            WorldManager.getInstance().setWorldPath(path);
            Debug.Log("Structure World Created at " + path);
            string dimensionPath = WorldLoadUtils.getDimensionFolderPath();
            Directory.CreateDirectory(dimensionPath);
            Debug.Log("Dimension Folder Created at " + dimensionPath);
            string structureDimPath = WorldLoadUtils.getDimPath(0);
            Directory.CreateDirectory(structureDimPath);
            GameObject structDimPrefab = Resources.Load<GameObject>("TileMaps/StructDimTileMap");
            Vector2Int caveSize = new Vector2Int(Mathf.Abs(structDimBounds.X.LowerBound-structDimBounds.X.UpperBound+1),Mathf.Abs(structDimBounds.Y.LowerBound-structDimBounds.Y.UpperBound+1));
            WorldTileConduitData dimData = WorldCreation.prefabToWorldTileConduitData(structDimPrefab,structDimBounds);
            WorldGenerationFactory.saveToJson(dimData,caveSize,structDimBounds,0,structureDimPath);


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

        public static void generateStructure(string structureName) {
            Structure structure = ScriptableObject.CreateInstance<Structure>();
            structure.name = structureName;
            string creationPath = Path.Combine(Global.EditorCreationPath,structureName);
            if (Directory.Exists(creationPath)) {
                Directory.Delete(creationPath,true);
            }
            Directory.CreateDirectory(creationPath);
            string path = getPath(structureName);
            string dimPath = WorldLoadUtils.getDimPath(path, 0);

            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.getUnloadedChunks(0,dimPath);
            Debug.Log(chunks.Count);

            
        }
    }
}


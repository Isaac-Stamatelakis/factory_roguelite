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
using Newtonsoft.Json;
using Tiles.TileMap.Interval;
using UI.Statistics;
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
        public const string PAREMETER_ID = "structure_parameter";
        public const string FILL_ID = "structure_fill";
        public const string EXPAND_ID = "structure_expand";
        public static string GetStructurePath(string structureName) {
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
            WorldManager.GetInstance().SetWorldName(path);
            Debug.Log("Structure World Created at " + path);
            
            string dimensionPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.DimensionFolder);
            Directory.CreateDirectory(dimensionPath);
            Debug.Log("Dimension Folder Created at " + dimensionPath);
            string structureDimPath = WorldLoadUtils.GetDimPath(0);
            Directory.CreateDirectory(structureDimPath);
            Vector2Int caveSize = bounds.GetSize();
            WorldTileConduitData dimData = WorldCreation.CreateEmptyWorldData(bounds);
            if (generationOption != null) {
                generationOption.apply(dimData);
            }
            WorldGenerationFactory.SaveToJson(dimData,caveSize,0,structureDimPath);
            PlayerInventoryData playerInventoryData = PlayerInventoryFactory.GetDefault();
            playerInventoryData.Inventory[0] = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(PAREMETER_ID),
                Global.MAX_SIZE, null);
            playerInventoryData.Inventory[1] = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(FILL_ID),
                Global.MAX_SIZE, null);
            playerInventoryData.Inventory[2] = new ItemSlot(ItemRegistry.GetInstance().GetItemObject(EXPAND_ID),
                Global.MAX_SIZE, null);
            
            PlayerData playerData = new PlayerData(
                dimensionData:null,
                playerRobot: RobotDataFactory.GetDefaultRobotData(),
                sInventoryData: PlayerInventoryFactory.Serialize(playerInventoryData),
                sRobotLoadOut: null,
                playerStatistics: new PlayerStatisticCollection(),
                miscPlayerData: WorldCreation.GetDefaultMiscPlayerData()
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Player,json);
        }
        
        /// <summary>
        /// This generates all structures inside a structure dev tool system.
        /// </summary>
        public static Structure LoadStructure(string structureName)
        {
            if (structureName == null) return null;
            string path = StructureGeneratorHelper.GetStructurePath(structureName);
            string dimPath = WorldLoadUtils.GetDimPath(path, 0);
            
            List<SoftLoadedConduitTileChunk> chunks = ChunkIO.GetUnloadedChunks(0,dimPath);
            
            Dictionary<Vector2Int, SoftLoadedConduitTileChunk> chunkDict = new Dictionary<Vector2Int, SoftLoadedConduitTileChunk>();
            IntervalVector coveredArea = new IntervalVector(new Interval<int>(0,0),new Interval<int>(0,0));
            foreach (SoftLoadedConduitTileChunk chunk in chunks) {
                if (chunkDict.ContainsKey(chunk.Position)) {
                    Debug.LogError($"Chunk dict already contains a chunk at position {chunk.Position}");
                    continue;
                }
                coveredArea.Add(chunk.Position);
                chunkDict[chunk.Position] = chunk;
            }   
            
            Vector2Int size = coveredArea.GetSize()*Global.CHUNK_SIZE;
            Vector2Int offset = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.CHUNK_SIZE;
            bool enforceEnclosure = false;
            bool[,] perimeter = new bool[size.x,size.y];
            foreach (SoftLoadedConduitTileChunk softLoadedConduitTileChunk in chunks) {
                foreach (IChunkPartition partition in softLoadedConduitTileChunk.Partitions) {
                    SeralizedWorldData data = partition.GetData();
                    for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                        for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                            string baseId = data.baseData.ids[x,y];
                            if (baseId == PAREMETER_ID) {
                                Vector2Int normalizedPosition = partition.GetRealPosition()*Global.CHUNK_PARTITION_SIZE+new Vector2Int(x,y)-offset;
                                perimeter[normalizedPosition.x,normalizedPosition.y] = true;
                                enforceEnclosure = true; // If even a single perimeter tile is in the map, enforce enclosure is on
                            }
                        }
                    }
                }
            }
            
            bool[,] visited = new bool[size.x,size.y];
            List<Vector2Int> directions = new List<Vector2Int>{
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down,
                Vector2Int.right
            };
            List<List<Vector2Int>> areas = new List<List<Vector2Int>>();
            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    if (!visited[x,y] && !perimeter[x,y]) {
                        List<Vector2Int> filledArea = DfsEnclosedArea(new Vector2Int(x,y),perimeter,visited,directions,size,enforceEnclosure);
                        if (filledArea != null) {
                            areas.Add(filledArea);
                        }
                    }
                }
            }

            List<StructureVariant> variants = new List<StructureVariant>();
            foreach (List<Vector2Int> area in areas) {
                if (area.Count == 0) {
                    continue;
                }
                Vector2Int first = area[0];
                IntervalVector boundingBox = new IntervalVector(new Interval<int>(first.x,first.x),new Interval<int>(first.y,first.y));
                for (int i = 1; i < area.Count; i++) {
                    boundingBox.Add(area[i]);
                }

                Vector2Int areaOffset = new Vector2Int(boundingBox.X.LowerBound,boundingBox.Y.LowerBound);
                Vector2Int areaSize = boundingBox.GetSize();
                WorldTileConduitData areaData = WorldGenerationFactory.CreateEmpty(areaSize);
                for (int x = 0; x < areaSize.x; x++) {
                    for (int y = 0; y < areaSize.y; y++) {
                        areaData.baseData.ids[x,y] = FILL_ID;
                    }
                }
                foreach (Vector2Int vector in area) {
                    Vector2Int adjustedVector = vector+offset;
                    Vector2Int chunkPosition = Global.GetChunkFromCell(adjustedVector);
                    IChunk chunk = chunkDict[chunkPosition];
                    Vector2Int partitionPosition = Global.GetPartitionFromCell(adjustedVector)-chunkPosition*Global.PARTITIONS_PER_CHUNK;
                    IChunkPartition partition = chunk.GetPartition(partitionPosition);
                    WorldTileConduitData partitionData = (WorldTileConduitData) partition.GetData();
                    Vector2Int posInPartition = Global.GetPositionInPartition(adjustedVector);
                    Vector2Int posInArea = vector-areaOffset;
                    WorldGenerationFactory.MapWorldTileConduitData(areaData,partitionData,posInArea,posInPartition);
                }
                variants.Add(new StructureVariant(
                    areaData,
                    areaSize
                ));
            }
            return new Structure(variants,coveredArea);
        }

        
        private static List<Vector2Int> DfsEnclosedArea(Vector2Int position, bool[,] perimeter, bool[,] visited, List<Vector2Int> directions, Vector2Int size, bool enforceEnclosure) {
            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            stack.Push(position);
            visited[position.x,position.y] = true;
            bool enclosed = true;
            List<Vector2Int> points = new List<Vector2Int>();
            points.Add(position);
            while (stack.Count > 0)
            {
                Vector2Int next = stack.Pop();
                foreach (Vector2Int direction in directions) {
                    Vector2Int directedPosition = next + direction;
                    bool outOfBounds = directedPosition.x < 0 || directedPosition.y < 0 || directedPosition.x >= size.x || directedPosition.y >= size.y;
                    if (outOfBounds) {
                        if (enforceEnclosure) {
                            enclosed = false;
                            points = null;
                        }
                        continue;
                    }
                    if (perimeter[directedPosition.x,directedPosition.y] || visited[directedPosition.x,directedPosition.y]) {
                        continue;
                    }
                    visited[directedPosition.x,directedPosition.y] = true;
                    stack.Push(directedPosition);
                    if (enclosed) {
                        points.Add(directedPosition);
                    }
                }
            }
            // points will be null if it is not enclosed, ie touched any out of bounds area
            return points;
        }
    }
}

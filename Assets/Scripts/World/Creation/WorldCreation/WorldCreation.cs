using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using Chunks.IO;
using Conduit.Placement.LoadOut;
using TileMaps.Layer;
using PlayerModule.IO;
using WorldModule.Caves;
using Items;
using RobotModule;
using Entities;
using DevTools.Structures;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Newtonsoft.Json;
using PlayerModule;
using TileEntity;
using TileEntity.Instances.CompactMachines;
using Tiles;
using Tiles.CustomTiles.IdTiles;
using UI.QuestBook;
using UI.Statistics;
using World.Serialization;
using Object = UnityEngine.Object;

namespace WorldModule {
    public struct WorldCreationData
    {
        public string WorldName;
        public string StructureName;
        public bool EnableCheats;
        public string QuestBook;

        public WorldCreationData(string worldName, string structureName, bool enableCheats, string questBook)
        {
            WorldName = worldName;
            StructureName = structureName;
            EnableCheats = enableCheats;
            QuestBook = questBook;
        }
    }

    public static class WorldCreation
    {
        public const bool ENABLE_PRESETS = true;
        public const string DIM_0_STRUCTURE_NAME = "Dim0";

        public static IEnumerator CreateWorld(WorldCreationData worldCreationData)
        {
            yield return ItemRegistry.LoadItems();
            WorldManager.getInstance().SetWorldName(worldCreationData.WorldName);
            string path = WorldLoadUtils.GetCurrentWorldPath();
            Directory.CreateDirectory(path);
            Debug.Log("World Folder Created at " + path);
            string mainPath = Path.Combine(path, WorldLoadUtils.CURRENT_FOLDER_PATH);
            string backUpPath = Path.Combine(path, WorldLoadUtils.BACKUP_FOLDER_PATH);
            Directory.CreateDirectory(mainPath);
            Directory.CreateDirectory(backUpPath);

            CompactMachineUtils.InitializeCompactMachineFolder();

            InitializeMetaData(WorldLoadUtils.GetMetaDataPath(worldCreationData.WorldName), worldCreationData);
            InitializeQuestBook(mainPath);
            InititalizeGameStages(WorldLoadUtils.GetWorldComponentPath(WorldFileType.GameStage));

            string dimensionFolderPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.DimensionFolder);
            Directory.CreateDirectory(dimensionFolderPath);
            Debug.Log("Dimension Folder Created at " + path);
            InitPlayerData(WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player));
            yield return InitDim0(worldCreationData.StructureName);
            WorldLoadUtils.createDimFolder(1);
        }

        public static void InitializeMetaData(string path, WorldCreationData worldCreationData)
        {
            WorldMetaData worldMetaData = new WorldMetaData(DateTime.Now, DateTime.Now, worldCreationData.EnableCheats,
                worldCreationData.QuestBook);
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(worldMetaData);
            byte[] compressed = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path, compressed);
        }

        public static void InititalizeGameStages(string path)
        {
            HashSet<string> stages = new HashSet<string>
            {
                "0"
            };
            GlobalHelper.SerializeCompressedJson(stages, path);
        }

        public static void InitializeQuestBook(string mainPath)
        {
            string folderPath = Path.Combine(mainPath, QuestBookUtils.WORLD_QUEST_FOLDER_PATH);
            Directory.CreateDirectory(folderPath);
        }

        private static void InitPlayerData(string path)
        {
            PlayerData playerData = GetDefaultPlayerData();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            byte[] compressed = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path, compressed);
        }

        public static PlayerData GetDefaultPlayerData()
        {
            return new PlayerData(
                dimensionData: null,
                playerRobot: RobotDataFactory.GetDefaultRobotData(),
                sInventoryData: PlayerInventoryFactory.Serialize(PlayerInventoryFactory.GetDefault()),
                sRobotLoadOut: null,
                playerStatistics: new PlayerStatisticCollection(),
                miscPlayerData: GetDefaultMiscPlayerData()
            );
        }

        public static MiscPlayerData GetDefaultMiscPlayerData()
        {
            return new MiscPlayerData
            {
                GrabbedItemData = null,
                ConduitPortPlacementLoadOuts = new Dictionary<LoadOutConduitType, IOConduitPortData>()

            };
        }

        public static void DeleteWorld(string name)
        {
            string path = WorldLoadUtils.GetWorldPath(name);
            if (!Directory.Exists(path))
            {
                Debug.LogWarning("Tried to delete world which does not exist " + path);
                return;
            }

            Debug.Log("World Folder Deleted at " + path);
            Directory.Delete(path, true);
        }


        public static IEnumerator InitDim0(string structureName)
        {
            if (WorldLoadUtils.DimExists(0))
            {
                Debug.LogError("Attempted to Initialize dim 0 when already exists");
                yield break;
            }

            WorldLoadUtils.createDimFolder(0);

            Structure structure = StructureGeneratorHelper.LoadStructure(structureName);
            IntervalVector dim0Bounds = GetDim0Bounds();

            WorldGenerationFactory.SaveToJson(structure.variants[0].Data, dim0Bounds.getSize(), 0,
                WorldLoadUtils.GetDimPath(0));
        }

        public static IntervalVector GetDim0Bounds()
        {
            return new IntervalVector(
                new Interval<int>(-2, 2),
                new Interval<int>(-2, 2)
            );
        }
        public static WorldTileConduitData CreateEmptyWorldData(IntervalVector bounds)
        {
            Vector2Int size = bounds.getSize() * Global.CHUNK_SIZE;
            int width = size.x;
            int height = size.y;
            SerializedBaseTileData baseData = SerializedTileDataFactory.createEmptyBaseData(width, height);
            SerializedBackgroundTileData backgroundData =
                SerializedTileDataFactory.createEmptyBackgroundData(width, height);
            SeralizedFluidTileData fluidTileData = SerializedTileDataFactory.createEmptyFluidData(width, height);
            SeralizedChunkConduitData itemData = SerializedTileDataFactory.createEmptyConduitData(width, height);
            SeralizedChunkConduitData fluidData = SerializedTileDataFactory.createEmptyConduitData(width, height);
            SeralizedChunkConduitData energyData = SerializedTileDataFactory.createEmptyConduitData(width, height);
            SeralizedChunkConduitData signalData = SerializedTileDataFactory.createEmptyConduitData(width, height);
            SeralizedChunkConduitData matrixData = SerializedTileDataFactory.createEmptyConduitData(width, height);
            return new WorldTileConduitData(baseData, backgroundData, new List<SeralizedEntityData>(), fluidTileData,
                itemData, fluidData, energyData, signalData, matrixData);
        }
    }
}

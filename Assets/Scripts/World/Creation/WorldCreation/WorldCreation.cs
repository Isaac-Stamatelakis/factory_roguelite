using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using Chunks.IO;
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
using Tiles;
using UI.QuestBook;
using World.Serialization;
using Object = UnityEngine.Object;

namespace WorldModule {
    public static class WorldCreation
    {
        public static IEnumerator CreateWorld(string name) {
            yield return ItemRegistry.LoadItems();
            WorldManager.getInstance().SetWorldName(name);
            string path = WorldLoadUtils.GetCurrentWorldPath();
            Directory.CreateDirectory(path);
            Debug.Log("World Folder Created at " + path);
            
            InitializeMetaData(WorldLoadUtils.GetWorldComponentPath(WorldFileType.Meta));
            InitializeQuestBook(WorldLoadUtils.GetWorldComponentPath(WorldFileType.Questbook));

            string dimensionFolderPath = WorldLoadUtils.GetWorldComponentPath(WorldFileType.DimensionFolder);
            Directory.CreateDirectory(dimensionFolderPath);
            Debug.Log("Dimension Folder Created at " + path);
            InitPlayerData(WorldLoadUtils.GetWorldComponentPath(WorldFileType.Player));
            yield return InitDim0();
            WorldLoadUtils.createDimFolder(1);
        }

        public static void InitializeMetaData(string path)
        {
            WorldMetaData worldMetaData =
            new WorldMetaData(DateTime.Now, DateTime.Now, new List<string>
            {
                "0"
            });
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(worldMetaData);
            byte[] compressed = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path, compressed);
        }

        public static void InitializeQuestBook(string path)
        {
            string defaultJson = File.ReadAllText(QuestBookUtils.DEFAULT_QUEST_BOOK_PATH);
            byte[] compressed = WorldLoadUtils.CompressString(defaultJson);
            File.WriteAllBytes(path, compressed);
        }

        private static void InitPlayerData(string path) {
            PlayerData playerData = new PlayerData(
                x: 0,
                y: 0,
                playerRobot: RobotDataFactory.GetDefaultRobotData(),
                name: "Izakio",
                sInventoryData: PlayerInventoryFactory.Serialize(PlayerInventoryFactory.GetDefault())
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            byte[] compressed = WorldLoadUtils.CompressString(json);
            File.WriteAllBytes(path, compressed);
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

        
        public static IEnumerator InitDim0() {
            if (WorldLoadUtils.DimExists(0)) {
                Debug.LogError("Attempted to Initialize dim 0 when already exists");
                yield break;
            }
            WorldLoadUtils.createDimFolder(0);
            
            Structure structure = StructureGeneratorHelper.LoadStructure("Dim0");
            IntervalVector dim0Bounds = GetDim0Bounds();
            
            WorldGenerationFactory.SaveToJson(structure.variants[0].Data,dim0Bounds.getSize(),0,WorldLoadUtils.GetDimPath(0));
        }

        public static IntervalVector GetDim0Bounds() {
            return new IntervalVector(
                new Interval<int>(-4,4),
                new Interval<int>(-3,3)
            );
        }
        
        public static IntervalVector GetTileMapChunkBounds(GameObject prefab) {
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            int xSize = (baseBounds.xMax-baseBounds.xMin)/Global.CHUNK_SIZE;
            int ySize = (baseBounds.yMax-baseBounds.yMin)/Global.CHUNK_SIZE;
            int xLower = (xSize - 1) / 2;
            int xUpper = xSize / 2;
            int yLower = (ySize - 1) / 2;
            int yUpper = ySize / 2;
            return new IntervalVector(new Interval<int>(xLower,xUpper), new Interval<int>(yLower,yUpper));
        }
        public static WorldTileConduitData PrefabToWorldTileConduitData(GameObject prefab, IntervalVector bounds) {
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            Vector2Int size = bounds.getSize();
            int width = size.x * Global.CHUNK_SIZE;
            int height = size.y * Global.CHUNK_SIZE;


            Tilemap backgroundTileMap = Global.findChild(prefab.transform,"Background").GetComponent<Tilemap>();

            Transform itemConduitTransform = prefab.transform.Find("ItemConduit");
            Tilemap itemConduitTileMap = null;
            if (itemConduitTransform != null) {
                itemConduitTileMap = itemConduitTransform.GetComponent<Tilemap>();
            }

            Transform fluidConduitTransform = prefab.transform.Find("FluidConduit");
            Tilemap fluidConduitTileMap = null;
            if (fluidConduitTransform != null) {
                fluidConduitTileMap = fluidConduitTransform.GetComponent<Tilemap>();
            }

            Transform energyConduitTransform = prefab.transform.Find("EnergyConduit");
            Tilemap energyConduitTileMap = null;
            if (energyConduitTransform != null) {
                energyConduitTileMap = energyConduitTransform.GetComponent<Tilemap>();
            }

            Transform signalConduitTransform = prefab.transform.Find("SignalConduit");
            Tilemap signalConduitTileMap = null;
            if (signalConduitTransform != null) {
                signalConduitTileMap = signalConduitTransform.GetComponent<Tilemap>();
            }
            Transform matrixConduitTransform = prefab.transform.Find("MatrixConduit");
            Tilemap matrixConduitTileMap = null;
            if (matrixConduitTransform != null) {
                matrixConduitTileMap = matrixConduitTransform.GetComponent<Tilemap>();
            }



            SerializedBaseTileData baseData = TileMapToSerializedChunkTileData(baseTileMap,width,height);
            SerializedBackgroundTileData backgroundData = TileMapToBackgroundTileData(backgroundTileMap,width,height);
            SeralizedFluidTileData fluidTileData = EmptyTileFluidData(width,height);
            SeralizedChunkConduitData itemData = TileMapToSerializedChunkConduitData(itemConduitTileMap,TileMapLayer.Item,width,height);
            SeralizedChunkConduitData fluidData = TileMapToSerializedChunkConduitData(fluidConduitTileMap,TileMapLayer.Fluid,width,height);
            SeralizedChunkConduitData energyData = TileMapToSerializedChunkConduitData(energyConduitTileMap,TileMapLayer.Energy,width,height);
            SeralizedChunkConduitData signalData = TileMapToSerializedChunkConduitData(signalConduitTileMap,TileMapLayer.Signal,width,height);
            SeralizedChunkConduitData matrixData = TileMapToSerializedChunkConduitData(matrixConduitTileMap,TileMapLayer.Matrix,width,height);
            return new WorldTileConduitData(
                baseData,
                backgroundData,
                new List<SeralizedEntityData>(),
                fluidTileData,
                itemData,
                fluidData,
                energyData,
                signalData,
                matrixData
            );
        }

        public static WorldTileConduitData CreateEmptyWorldData(IntervalVector bounds) {
            Vector2Int size = bounds.getSize()*Global.CHUNK_SIZE;
            int width = size.x;
            int height = size.y;
            SerializedBaseTileData baseData = SerializedTileDataFactory.createEmptyBaseData(width,height);
            SerializedBackgroundTileData backgroundData = SerializedTileDataFactory.createEmptyBackgroundData(width,height);
            SeralizedFluidTileData fluidTileData = SerializedTileDataFactory.createEmptyFluidData(width,height);
            SeralizedChunkConduitData itemData = SerializedTileDataFactory.createEmptyConduitData(width,height);
            SeralizedChunkConduitData fluidData = SerializedTileDataFactory.createEmptyConduitData(width,height);
            SeralizedChunkConduitData energyData = SerializedTileDataFactory.createEmptyConduitData(width,height);
            SeralizedChunkConduitData signalData = SerializedTileDataFactory.createEmptyConduitData(width,height);
            SeralizedChunkConduitData matrixData = SerializedTileDataFactory.createEmptyConduitData(width,height);
            return new WorldTileConduitData(baseData,backgroundData,new List<SeralizedEntityData>(),fluidTileData,itemData,fluidData,energyData,signalData,matrixData);
        }
    private static SerializedBaseTileData TileMapToSerializedChunkTileData(Tilemap tilemap, int width, int height) {
        SerializedBaseTileData data = new SerializedBaseTileData();
        string[,] ids = new string[width,height];
        string[,] sTileEntityOptions = new string[width,height];
        BaseTileData[,] sTileOptions = new BaseTileData[width,height];
    
        BoundsInt bounds = tilemap.cellBounds;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x+bounds.xMin, y+bounds.yMin, 0);
                if (tilemap.HasTile(tilePosition))
                {
                    TileBase tile = tilemap.GetTile(tilePosition);
                    if (tile is IIDTile) {
                        string id = ((IIDTile) tile).getId();
                        if (string.IsNullOrEmpty(id)) {
                            continue;
                        }
                        ids[x,y]= id;
                    }
                }
            }
            
        }
        data.ids = ids;
        data.sTileEntityOptions = sTileEntityOptions;
        data.sTileOptions = sTileOptions;
        return data;
    }

    private static SerializedBackgroundTileData TileMapToBackgroundTileData(Tilemap tilemap, int width, int height) {
        ItemRegistry itemRegistry = ItemRegistry.GetInstance();
        //Debug.Log("Generating SerializedBackgroundTileData");
        SerializedBackgroundTileData data = new SerializedBackgroundTileData();
        string[,] ids = new string[width,height];
        if (tilemap != null) {
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x+bounds.xMin, y+bounds.yMin, 0);
                    if (tilemap.HasTile(tilePosition))
                    {
                        TileBase tile = tilemap.GetTile(tilePosition);
                        if (tile is IIDTile) {
                            string id = ((IIDTile) tile).getId();
                            if (id is null or "") {
                                continue;
                            }
                            ids[x,y]= id;
                        }
                    }
                }
            }
        }
        data.ids = ids;
        return data;
    }

    private static SeralizedFluidTileData TileMapToFluidTileData(Tilemap tilemap, int width, int height) {
        ItemRegistry itemRegistry = ItemRegistry.GetInstance();
        Debug.Log("Generating SeralizedFluidTileData");
        SeralizedFluidTileData data = new SeralizedFluidTileData();
        float[,] fill = new float[width,height];
        string[,] ids = new string[width,height];
        if (tilemap != null) {
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x+bounds.xMin, y+bounds.yMin, 0);
                    if (tilemap.HasTile(tilePosition))
                    {
                        TileBase tile = tilemap.GetTile(tilePosition);
                        if (tile is IIDTile) {
                            string id = ((IIDTile) tile).getId();
                            if (id is null or "") {
                                continue;
                            }
                            fill[x,y] = 1;
                            ids[x,y]= id;
                        }
                    }
                }
            }
        }
        data.ids = ids;
        data.fill = fill;
        return data;
    }

    private static SeralizedFluidTileData EmptyTileFluidData(int width, int height) {
        SeralizedFluidTileData data = new SeralizedFluidTileData();
        data.ids = new string[width,height];
        data.fill = new float[width,height];
        return data;
    }
    private static SeralizedChunkConduitData TileMapToSerializedChunkConduitData(Tilemap tilemap, TileMapLayer layer, int width, int height) {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            //Debug.Log("Generating SerializedChunkConduitData for " + layer.ToString());
            SeralizedChunkConduitData data = new SeralizedChunkConduitData();
            string[,] ids = new string[width,height];
            string[,] conduitOptions = new string[width,height];
            
            BoundsInt bounds = tilemap.cellBounds;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x+bounds.xMin, y+bounds.yMin, 0);
                    if (tilemap.HasTile(tilePosition))
                    {
                        TileBase tile = tilemap.GetTile(tilePosition);
                        if (tile is StandardTile) {
                            string id = ((StandardTile) tile).id;
                            if (id is null or "") {
                                continue;
                            }
                            ids[x,y]= id;
                        }
                    }
                }
            }
            
            data.ids = ids;
            data.conduitOptions = conduitOptions;
            return data;
        }
    }

}

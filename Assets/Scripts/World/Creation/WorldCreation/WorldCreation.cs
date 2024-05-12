using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using ChunkModule.IO;
using TileMapModule.Layer;
using PlayerModule.IO;
using WorldModule.Caves;
using ItemModule;
using RobotModule;
using Entities;

namespace WorldModule {
    public static class WorldCreation
    {
        public static void createWorld(string name) {
            WorldManager.getInstance().setWorldPath(Path.Combine(WorldLoadUtils.DefaultWorldFolder,name));
            string path = WorldLoadUtils.getFullWorldPath();
            Directory.CreateDirectory(path);
            Debug.Log("World Folder Created at " + path);
            string dimensionFolderPath = Path.Combine(path,WorldLoadUtils.DimensionFolderName);
            Directory.CreateDirectory(dimensionFolderPath);
            Debug.Log("Dimension Folder Created at " + path);
            initPlayerData(WorldLoadUtils.getPlayerDataPath());
            initDim0();
            WorldLoadUtils.createDimFolder(1);
        }
        

        public static void initPlayerData(string path) {
            PlayerData playerData = new PlayerData(
                x: 0,
                y: 0,
                playerRobot: RobotDataFactory.getDefaultRobotString(false),
                name: "Izakio",
                inventoryJson: ItemSlotFactory.createEmptySerializedInventory(40)
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            File.WriteAllText(path,json);
        }

        
        public static void initDim0() {
            if (WorldLoadUtils.dimExists(0)) {
                Debug.LogError("Attempted to init dim 0 when already exists");
                return;
            }
            WorldLoadUtils.createDimFolder(0);
            GameObject dim0Prefab = Resources.Load<GameObject>("TileMaps/Dim0");
            IntervalVector dim0Bounds = getDim0Bounds();
            Vector2Int caveSize = new Vector2Int(Mathf.Abs(dim0Bounds.X.LowerBound-dim0Bounds.X.UpperBound+1),Mathf.Abs(dim0Bounds.Y.LowerBound-dim0Bounds.Y.UpperBound+1));
            SerializedTileConduitData dim0Data = prefabToWorldTileConduitData(dim0Prefab,dim0Bounds);
            WorldGenerationFactory.saveToJson(dim0Data,caveSize,dim0Bounds,0,WorldLoadUtils.getDimPath(0));
        }

        public static IntervalVector getDim0Bounds() {
            return new IntervalVector(
                new Interval<int>(-4,4),
                new Interval<int>(-3,3)
            );
        }
        
        public static IntervalVector getTileMapChunkBounds(GameObject prefab) {
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            int xSize = (baseBounds.xMax-baseBounds.xMin)/Global.ChunkSize;
            int ySize = (baseBounds.yMax-baseBounds.yMin)/Global.ChunkSize;
            int xLower = (xSize - 1) / 2;
            int xUpper = xSize / 2;
            int yLower = (ySize - 1) / 2;
            int yUpper = ySize / 2;
            return new IntervalVector(new Interval<int>(xLower,xUpper), new Interval<int>(yLower,yUpper));
        }
        public static SerializedTileConduitData prefabToWorldTileConduitData(GameObject prefab, IntervalVector bounds) {
            
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            int width = (Mathf.Abs(bounds.X.LowerBound-bounds.X.UpperBound)+1) * Global.ChunkSize;
            int height = (Mathf.Abs(bounds.Y.LowerBound-bounds.Y.UpperBound)+1) * Global.ChunkSize;


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



            SerializedBaseTileData baseData = tileMapToSerializedChunkTileData(baseTileMap,width,height);
            SerializedBackgroundTileData backgroundData = tileMapToBackgroundTileData(backgroundTileMap,width,height);
            SeralizedFluidTileData fluidTileData = emptyTileFluidData(width,height);
            SeralizedChunkConduitData itemData = tileMapToSerializedChunkConduitData(itemConduitTileMap,TileMapLayer.Item,width,height);
            SeralizedChunkConduitData fluidData = tileMapToSerializedChunkConduitData(fluidConduitTileMap,TileMapLayer.Fluid,width,height);
            SeralizedChunkConduitData energyData = tileMapToSerializedChunkConduitData(energyConduitTileMap,TileMapLayer.Energy,width,height);
            SeralizedChunkConduitData signalData = tileMapToSerializedChunkConduitData(signalConduitTileMap,TileMapLayer.Signal,width,height);
            SeralizedChunkConduitData matrixData = tileMapToSerializedChunkConduitData(matrixConduitTileMap,TileMapLayer.Matrix,width,height);
            return new SerializedTileConduitData(
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
        private static SerializedBaseTileData tileMapToSerializedChunkTileData(Tilemap tilemap, int width, int height) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Debug.Log("Generating SerializedChunkTileData for Base");
            SerializedBaseTileData data = new SerializedBaseTileData();
            string[,] ids = new string[width,height];
            string[,] sTileEntityOptions = new string[width,height];
            string[,] sTileOptions = new string[width,height];
        
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
                            if (id == null || id == "") {
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

    private static SerializedBackgroundTileData tileMapToBackgroundTileData(Tilemap tilemap, int width, int height) {
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        Debug.Log("Generating SerializedBackgroundTileData");
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
                            if (id == null || id == "") {
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

    private static SeralizedFluidTileData tileMapToFluidTileData(Tilemap tilemap, int width, int height) {
        ItemRegistry itemRegistry = ItemRegistry.getInstance();
        Debug.Log("Generating SeralizedFluidTileData");
        SeralizedFluidTileData data = new SeralizedFluidTileData();
        int[,] fill = new int[width,height];
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
                            if (id == null || id == "") {
                                continue;
                            }
                            fill[x,y] = 8;
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

    private static SeralizedFluidTileData emptyTileFluidData(int width, int height) {
        SeralizedFluidTileData data = new SeralizedFluidTileData();
        data.ids = new string[width,height];
        data.fill = new int[width,height];
        return data;
    }
    private static SeralizedChunkConduitData tileMapToSerializedChunkConduitData(Tilemap tilemap, TileMapLayer layer, int width, int height) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Debug.Log("Generating SerializedChunkConduitData for " + layer.ToString());
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
                            if (id == null || id == "") {
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

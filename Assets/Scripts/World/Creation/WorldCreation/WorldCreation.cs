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

namespace WorldModule {
    public static class WorldCreation
    {
        public static IEnumerator createWorld(string name) {
            yield return ItemRegistry.loadItems();
            WorldManager.getInstance().setWorldPath(Path.Combine(WorldLoadUtils.DefaultWorldFolder,name));
            string path = WorldLoadUtils.getFullWorldPath();
            Directory.CreateDirectory(path);
            Debug.Log("World Folder Created at " + path);
            string dimensionFolderPath = Path.Combine(path,WorldLoadUtils.DimensionFolderName);
            Directory.CreateDirectory(dimensionFolderPath);
            Debug.Log("Dimension Folder Created at " + path);
            initPlayerData(WorldLoadUtils.getPlayerDataPath());
            yield return initDim0();
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

        
        public static IEnumerator initDim0() {
            if (WorldLoadUtils.dimExists(0)) {
                Debug.LogError("Attempted to init dim 0 when already exists");
                yield break;
            }
            WorldLoadUtils.createDimFolder(0);
            List<string> labels = new List<string>{"dim0","structure"};
            AsyncOperationHandle<IList<Object>> handle = Addressables.LoadAssetsAsync<Object>(labels,null,Addressables.MergeMode.Intersection);
            yield return handle;
            List<Structure> structures = AddressableUtils.validateHandle<Structure>(handle);
            if (structures.Count == 0) {
                Debug.LogError("Could not init dim 0. No structure with tags 'dim0' and 'structure'");
                yield break;
            }
            if (structures.Count > 1) {
                Debug.LogWarning("Multiple structures with tags 'dim0' and 'structure");
            }
            Structure structure = structures[0];
    
            if (structure.variants.Count == 0) {
                Debug.LogWarning("Dim0 structure contains no structure variants");
                yield break;
            }

            StructureVariant variant = structure.variants[0];
            Vector2Int dimSize = getDim0Bounds().getSize()*Global.ChunkSize;
            if (variant.Size != dimSize) {
                Debug.LogError($"Structure for dim0 size {variant.Size} does not match dim0 size {dimSize}");
                yield break;
            }

            WorldTileConduitData dim0Data = JsonConvert.DeserializeObject<WorldTileConduitData>(variant.Data);
            IntervalVector dim0Bounds = getDim0Bounds();
            //WorldTileConduitData dim0Data = prefabToWorldTileConduitData(dim0Prefab,dim0Bounds);
            WorldGenerationFactory.saveToJson(dim0Data,dim0Bounds.getSize(),0,WorldLoadUtils.getDimPath(0));
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
        public static WorldTileConduitData prefabToWorldTileConduitData(GameObject prefab, IntervalVector bounds) {
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            Vector2Int size = bounds.getSize();
            int width = size.x * Global.ChunkSize;
            int height = size.y * Global.ChunkSize;


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

        public static WorldTileConduitData createEmptyWorldData(IntervalVector bounds) {
            Vector2Int size = bounds.getSize()*Global.ChunkSize;
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
        private static SerializedBaseTileData tileMapToSerializedChunkTileData(Tilemap tilemap, int width, int height) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            //Debug.Log("Generating SerializedChunkTileData for Base");
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
                            if (id == null || id == "") {
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

    private static SeralizedFluidTileData emptyTileFluidData(int width, int height) {
        SeralizedFluidTileData data = new SeralizedFluidTileData();
        data.ids = new string[width,height];
        data.fill = new float[width,height];
        return data;
    }
    private static SeralizedChunkConduitData tileMapToSerializedChunkConduitData(Tilemap tilemap, TileMapLayer layer, int width, int height) {
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using ChunkModule.IO;

namespace WorldDataModule {
    public static class WorldCreation
    {
        public static bool worldExists(string name) {
            string path = getWorldPath(name);
            return folderExists(path);
        }

        public static bool folderExists(string path) {
            return File.Exists(path) || Directory.Exists(path);
        }

        public static void createWorld(string name) {
            Global.WorldName=name;
            string path = getWorldPath(name);
            Directory.CreateDirectory(path);
            Debug.Log("World Folder Created at " + path);
            Directory.CreateDirectory(path + "/Dimensions");
            Debug.Log("Dimension Folder Created at " + path);
            initPlayerData(name);
            initDim0(name);
        }
        public static string getWorldPath(string name) {
            return Application.persistentDataPath + "/worlds/" + name; 
        }

        public static void initPlayerData(string name) {
            
            PlayerData playerData = new PlayerData(
                x: 0,
                y: 0,
                robotID: "happy_mk1",
                name: "Izakio",
                inventoryJson: ItemSlotFactory.createEmptySerializedInventory(40)
            );
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerData);
            string path = getPlayerDataPath(name);
            File.WriteAllText(path,json);
        }

        public static string getPlayerDataPath(string name) {
            return getWorldPath(name) + "/player_data.json";;
        }
        public static string getDimensionFolderPath(string name) {
            return getWorldPath(name) + "/Dimensions";
        }

        public static bool dimExists(string name, int dim) {
            string path = getDimPath(name, dim);
            return folderExists(path);
        }
        public static void createDimFolder(string worldName, int dim) {
            string path = getDimPath(worldName,dim);
            Directory.CreateDirectory(path);
        }

        public static string getDimPath(string worldName, int dim) {
            return getDimensionFolderPath(worldName) + "/dim" + dim;
        }

        public static void initDim0(string name) {
            if (dimExists(name,0)) {
                Debug.LogError("Attempted to init dim 0 when already exists");
            }
            createDimFolder(name,0);
            GameObject dim0Prefab = Resources.Load<GameObject>("TileMaps/Dim0");
            Cave cave = new Cave();
            IntervalVector dim0Bounds = getDim0Bounds();

            CaveArea caveArea = new CaveArea(
                new Vector2Int(dim0Bounds.X.LowerBound,dim0Bounds.X.UpperBound),
                new Vector2Int(dim0Bounds.Y.LowerBound,dim0Bounds.Y.UpperBound)
            );
            WorldTileData dim0Data = prefabToWorldTileData(dim0Prefab,caveArea);
            cave.areas = new List<CaveArea> {
                caveArea
            };
            ProcGenHelper.saveToJson(dim0Data,cave,0);
        }

        public static IntervalVector getDim0Bounds() {
            return new IntervalVector(
                new Interval<int>(-4,4),
                new Interval<int>(-3,3)
            );
        }
        public static WorldTileData prefabToWorldTileData(GameObject prefab, CaveArea caveArea) {
            Tilemap baseTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            BoundsInt baseBounds = baseTileMap.cellBounds;
            int width = (Mathf.Abs(caveArea.xInterval.y-caveArea.xInterval.x)+1) * Global.ChunkSize;
            int height = (Mathf.Abs(caveArea.yInterval.y-caveArea.yInterval.x)+1) * Global.ChunkSize;
            Tilemap backgroundTileMap = Global.findChild(prefab.transform,"Base").GetComponent<Tilemap>();
            SeralizedChunkTileData baseData = tileMapToSerializedChunkTileData(baseTileMap,TileMapLayer.Base,width,height);
            SeralizedChunkTileData backgroundData = tileMapToSerializedChunkTileData(baseTileMap,TileMapLayer.Background,width,height);
            return new WorldTileData(
                new List<EntityData>(),
                baseData,
                backgroundData
            );
        }
        public static SeralizedChunkTileData tileMapToSerializedChunkTileData(Tilemap tilemap, TileMapLayer layer, int width, int height) {
            BoundsInt bounds = tilemap.cellBounds;
            ItemRegistry itemRegistry = ItemRegistry.getInstance();
            Debug.Log("Generating SerializedChunkTileData for " + tilemap.name);
            SeralizedChunkTileData data = new SeralizedChunkTileData();
            List<List<string>> ids = new List<List<string>>();
            List<List<string>> sTileEntityOptions = new List<List<string>>();
            List<List<string>> sTileOptions = new List<List<string>>();
            for (int x = 0; x < width; x ++) {
                List<string> tempIds = new List<string>();
                List<string> tempSTileEntityOptions = new List<string>();
                List<string> tempSTileOptions = new List<string>();
                for (int y = 0; y < height; y++) {
                    tempIds.Add(null);
                    tempSTileEntityOptions.Add(null);
                    tempSTileOptions.Add(null);
                }
                ids.Add(tempIds);
                sTileEntityOptions.Add(tempSTileEntityOptions);
                sTileOptions.Add(tempSTileOptions);
            }
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
                            ids[x][y]= id;
                        }
                    }
                }
            }
            data.ids = ids;
            data.sTileEntityOptions = sTileEntityOptions;
            data.sTileOptions = sTileOptions;
            return data;
        }
    }

}

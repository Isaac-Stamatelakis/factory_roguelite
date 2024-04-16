using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule.Type;
using Fluids;
using TileMapModule;
using TileMapModule.Conduit;

namespace ChunkModule.ClosedChunkSystemModule {
    public static class TileMapBundleFactory 
    {
        public static List<TileMapType> getStandardTileTypes() {
            return new List<TileMapType>{
                TileMapType.Block,
                TileMapType.Background,
                TileMapType.Object,
                TileMapType.Platform,
                TileMapType.SlipperyBlock,
                TileMapType.ColladableObject,
                TileMapType.ClimableObject
            };
        }
        public static List<TileMapType> getConduitTileTypes() {
            return new List<TileMapType>{
                TileMapType.ItemConduit,
                TileMapType.FluidConduit,
                TileMapType.SignalConduit,
                TileMapType.EnergyConduit,
                TileMapType.MatrixConduit
            };
        }

        public static void loadTileSystemMaps(Transform systemTransform, Dictionary<TileMapType, ITileMap> tileGridMaps) {
            List<TileMapType> standardMaps = TileMapBundleFactory.getStandardTileTypes();
            foreach (TileMapType tileMapType in standardMaps) {
                initTileMapContainer(tileMapType,systemTransform,tileGridMaps);
            }
            initTileMapContainer(TileMapType.Fluid,systemTransform,tileGridMaps);
            tileGridMaps[TileMapType.Block].addListener((ITileMapListener)tileGridMaps[TileMapType.Fluid]);
            tileGridMaps[TileMapType.SlipperyBlock].addListener((ITileMapListener)tileGridMaps[TileMapType.Fluid]);
            
        }

        public static void loadConduitSystemMaps(Transform systemTransform, Dictionary<TileMapType, ITileMap> tileGridMaps) {
            loadTileSystemMaps(systemTransform,tileGridMaps);
            List<TileMapType> conduitMaps = TileMapBundleFactory.getConduitTileTypes();
            foreach (TileMapType tileMapType in conduitMaps) {
                initTileMapContainer(tileMapType,systemTransform,tileGridMaps);
            }
        }

        private static void initTileMapContainer(TileMapType tileType, Transform parent, Dictionary<TileMapType, ITileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.transform.SetParent(parent);
            container.name = tileType.ToString();
            container.layer = LayerMask.NameToLayer(tileType.ToString());
            container.transform.localPosition = new Vector3(0,0,tileType.getZValue());
            Grid grid = container.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.5f,0.5f,1f);
            if (tileType.isTile()) {
                TileGridMap tileGridMap = container.AddComponent<TileGridMap>();
                tileGridMap.type = tileType;
                tileGridMaps[tileType] = tileGridMap;
            } else if (tileType.isConduit()) {
                ConduitTileMap tileGridMap = container.AddComponent<ConduitTileMap>();
                tileGridMap.type = tileType;
                tileGridMaps[tileType] = tileGridMap;
            } else if (tileType.isFluid()) {
                FluidTileMap fluidTileMap = container.AddComponent<FluidTileMap>();
                fluidTileMap.type = TileMapType.Fluid;
                tileGridMaps[tileType] = fluidTileMap;
            }
        }
    }
}


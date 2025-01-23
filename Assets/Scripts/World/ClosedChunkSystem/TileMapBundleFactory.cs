using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using Fluids;
using TileMaps;
using TileMaps.Conduit;
using Unity.VisualScripting;

namespace Chunks.Systems {
    public static class TileMapBundleFactory 
    {
        public static List<TileMapType> getStandardTileTypes() {
            return new List<TileMapType>{
                TileMapType.Block,
                TileMapType.Background,
                TileMapType.Object,
                TileMapType.Platform,
                TileMapType.ColladableObject,
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

        public static void LoadTileSystemMaps(Transform systemTransform, Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.name = "Tiles";
            container.transform.SetParent(systemTransform);
            List<TileMapType> standardMaps = TileMapBundleFactory.getStandardTileTypes();
            foreach (TileMapType tileMapType in standardMaps) {
                InitTileMapContainer(tileMapType,container.transform,tileGridMaps);
            }
            InitTileMapContainer(TileMapType.Fluid,systemTransform,tileGridMaps);
            tileGridMaps[TileMapType.Block].addListener((ITileMapListener)tileGridMaps[TileMapType.Fluid]);
            
        }

        public static void LoadConduitSystemMaps(Transform systemTransform, Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.name = "Conduits";
            container.transform.SetParent(systemTransform);
            LoadTileSystemMaps(systemTransform,tileGridMaps);
            List<TileMapType> conduitMaps = TileMapBundleFactory.getConduitTileTypes();
            foreach (TileMapType tileMapType in conduitMaps) {
                InitTileMapContainer(tileMapType,container.transform,tileGridMaps);
            }
        }

        private static void InitTileMapContainer(TileMapType tileType, Transform parent, Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.transform.SetParent(parent);
            container.name = tileType.ToString();
            if (tileType.hasCollider()) {
                container.layer = LayerMask.NameToLayer(tileType.ToString());
            }
            container.transform.localPosition = new Vector3(0,0,tileType.getZValue());
            Grid grid = container.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.5f,0.5f,1f);

            var worldTileMap = CreateTileMap(tileType, container.transform);
            tileGridMaps[tileType] = worldTileMap;
            tileGridMaps[tileType].Initialize(tileType);
        }

        private static IWorldTileMap CreateTileMap(TileMapType tileType, Transform container)
        {
            if (tileType.isTile()) {
                WorldTileGridMap worldTileGridMap;
                if (tileType == TileMapType.Block) {
                    worldTileGridMap = container.AddComponent<OutlineWorldTileGridMap>();
                } else if (tileType == TileMapType.Background) {
                    worldTileGridMap = container.AddComponent<BackgroundWorldTileMap>();
                } else {
                    worldTileGridMap = container.AddComponent<WorldTileGridMap>();
                }

                return worldTileGridMap;
                
            } else if (tileType.isConduit()) {
                return container.AddComponent<ConduitTileMap>();
            } else if (tileType.isFluid()) {
                return container.AddComponent<FluidIWorldTileMap>();
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}


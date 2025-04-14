using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMaps.Type;
using Fluids;
using TileMaps;
using TileMaps.Conduit;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

namespace Chunks.Systems {
    
    public enum TileEntityTileMapType
    {
        LitFront,
        UnLitFront,
        LitBack,
        UnLitBack,
    }
    public static class TileMapBundleFactory
    {
        private const float TILE_SIZE = 0.5f;
        public static List<TileMapType> getStandardTileTypes() {
            return new List<TileMapType>{
                TileMapType.Block,
                TileMapType.Background,
                TileMapType.Object,
                TileMapType.Platform
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
            tileGridMaps[TileMapType.Block].AddListener((ITileMapListener)tileGridMaps[TileMapType.Fluid]);
            tileGridMaps[TileMapType.Block].AddListener((ITileMapListener)tileGridMaps[TileMapType.Object]);
            tileGridMaps[TileMapType.Block].AddListener((ITileMapListener)tileGridMaps[TileMapType.Block]);
            tileGridMaps[TileMapType.Object].AddListener((ITileMapListener)tileGridMaps[TileMapType.Object]);
            tileGridMaps[TileMapType.Object].AddListener((ITileMapListener)tileGridMaps[TileMapType.Fluid]);

        }

        public static void LoadConduitSystemMaps(Transform systemTransform, Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.name = "Conduits";
            container.transform.SetParent(systemTransform);
            List<TileMapType> conduitMaps = TileMapBundleFactory.getConduitTileTypes();
            foreach (TileMapType tileMapType in conduitMaps) {
                InitTileMapContainer(tileMapType,container.transform,tileGridMaps);
            }
        }

        private static void InitTileMapContainer(TileMapType tileType, Transform parent, Dictionary<TileMapType, IWorldTileMap> tileGridMaps) {
            GameObject container = new GameObject();
            container.transform.SetParent(parent);
            container.tag = "Ground";
            container.name = tileType.ToString();
            if (tileType.HasCollider()) {
                container.layer = LayerMask.NameToLayer(tileType.ToString());
            }
            
            container.transform.localPosition = new Vector3(0,0,tileType.GetZValue());
            Grid grid = container.AddComponent<Grid>();
            grid.cellSize = new Vector3(TILE_SIZE,TILE_SIZE,1f);

            var worldTileMap = CreateTileMap(tileType, container.transform);
            tileGridMaps[tileType] = worldTileMap;
            tileGridMaps[tileType].Initialize(tileType);
            
            if (tileType is TileMapType.Block or TileMapType.Platform or TileMapType.Object or TileMapType.Fluid)
            {
                GameObject gameObject = worldTileMap.GetTilemap().gameObject;
                Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                gameObject.GetComponent<TilemapCollider2D>().usedByComposite = true;
                CompositeCollider2D compositeCollider2D = gameObject.AddComponent<CompositeCollider2D>();
                compositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
                if (tileType is TileMapType.Fluid)
                {
                    compositeCollider2D.isTrigger = true;
                }
            }

            
            
        }

        

        private static void LoadTileEntityDisplayTileMap(TileEntityTileMapType tileEntityTileMapType, Transform parent, Dictionary<TileEntityTileMapType, Tilemap> tilemaps, Material litMaterial)
        {
            GameObject container = new GameObject();
            container.transform.SetParent(parent);
            container.name = tileEntityTileMapType.ToString();
            float epsilon = 0.1f;
            switch (tileEntityTileMapType)
            {
                case TileEntityTileMapType.LitFront:
                case TileEntityTileMapType.UnLitFront:
                    epsilon *= -1;
                    break;
            }
            container.transform.localPosition = new Vector3(0, 0, TileMapType.Object.GetZValue() + epsilon);
            Grid grid = container.AddComponent<Grid>();
            grid.cellSize = new Vector3(TILE_SIZE,TILE_SIZE,1f);
            Tilemap tilemap = container.AddComponent<Tilemap>();
            TilemapRenderer renderer = container.AddComponent<TilemapRenderer>();
            switch (tileEntityTileMapType)
            {
                case TileEntityTileMapType.LitFront:
                case TileEntityTileMapType.LitBack:
                    renderer.material = litMaterial;
                    break;
            }
            tilemaps[tileEntityTileMapType] = tilemap;

        }

        public static void LoadTileEntityMaps(Transform parent, Dictionary<TileEntityTileMapType, Tilemap> tilemaps, Material litMaterial)
        {
            GameObject gameObject = new GameObject();
            gameObject.name = "TileEntityMaps";
            gameObject.transform.SetParent(parent);
            LoadTileEntityDisplayTileMap(TileEntityTileMapType.LitFront, gameObject.transform, tilemaps,litMaterial);
            LoadTileEntityDisplayTileMap(TileEntityTileMapType.UnLitFront, gameObject.transform, tilemaps,litMaterial);
            LoadTileEntityDisplayTileMap(TileEntityTileMapType.LitBack, gameObject.transform, tilemaps,litMaterial);
            LoadTileEntityDisplayTileMap(TileEntityTileMapType.UnLitBack, gameObject.transform, tilemaps,litMaterial);
            
        }

        private static IWorldTileMap CreateTileMap(TileMapType tileType, Transform container)
        {
            if (tileType.IsTile())
            {
                WorldTileMap worldTileMap = tileType switch
                {
                    TileMapType.Block => container.AddComponent<BlockWorldTileMap>(),
                    TileMapType.Background => container.AddComponent<BackgroundWorldTileMap>(),
                    _ => container.AddComponent<WorldTileMap>()
                };

                return worldTileMap;
            } else if (tileType.IsConduit()) {
                return container.AddComponent<ConduitTileMap>();
            } else if (tileType.IsFluid()) {
                return container.AddComponent<FluidTileMap>();
            }

            throw new ArgumentOutOfRangeException();
        }
        
    }
}


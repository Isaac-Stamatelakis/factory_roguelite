using System.Collections;
using System.Collections.Generic;
using Chunks.Partitions;
using Items;
using TileMaps.Type;
using UnityEngine;
using Tiles;
using Tiles.Options.Overlay;
using Tiles.TileMap;

namespace TileMaps {
    public class BackgroundWorldTileMap : WorldTileMap, IWorldShaderTilemap
    {
        private ShaderTilemapManager shaderTilemapManager;
        
        const float BOUNDARY_SIZE = Global.TILE_SIZE/4;
        private readonly List<Vector2> directions = new()
        {
            new Vector2(-BOUNDARY_SIZE,0),
            new Vector2(BOUNDARY_SIZE,0),
            new Vector2(0,BOUNDARY_SIZE),
            new Vector2(0,-BOUNDARY_SIZE),
            new Vector2(-BOUNDARY_SIZE,BOUNDARY_SIZE),
            new Vector2(BOUNDARY_SIZE,-BOUNDARY_SIZE),
            new Vector2(BOUNDARY_SIZE,BOUNDARY_SIZE),
            new Vector2(-BOUNDARY_SIZE,-BOUNDARY_SIZE)
        };

        protected override void SetTile(int x, int y, TileItem tileItem)
        {
            Vector2Int position = new Vector2Int(x, y);
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition == null) return; // Might need this?
            
            var transmutableMaterial = tileItem.tileOptions.TransmutableColorOverride;
            Vector3Int placementPositon = new Vector3Int(x, y, 0);
            
            PlaceTileInTilemap(tilemap, tileItem, placementPositon, partition);
            if (!transmutableMaterial) return;
            Material material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(transmutableMaterial);
            if (material)
            {
                PlaceTileInTilemap(shaderTilemapManager.GetTileMap(material), tileItem, placementPositon, partition);
            }
        }

        protected override void RemoveTile(int x, int y)
        {
            Vector3Int vector3 = new Vector3Int(x, y, 0);
            if (!tilemap.HasTile(vector3)) return;
            tilemap.SetTile(vector3,null);
            shaderTilemapManager.ClearAllOnTile(ref vector3);
        }

        public override void Initialize(TileMapType type)
        {
            base.Initialize(type);
            shaderTilemapManager = new ShaderTilemapManager(transform, -0.1f, false, TileMapType.Background,2);
        }

        public override Vector2Int GetHitTilePosition(Vector2 position) {
            Vector3Int cellPosition = tilemap.WorldToCell(position);
            if (tilemap.GetTile(cellPosition)) { // Mouse was over 16x16 area of background
                return (Vector2Int)cellPosition;
            }
            foreach (Vector2 direction in directions) {
                cellPosition = tilemap.WorldToCell(direction+position);
                cellPosition.z = 0;
                if (tilemap.GetTile(cellPosition)) {
                    return (Vector2Int)cellPosition;
                }
            }
            return (Vector2Int)cellPosition;


        }

        public ShaderTilemapManager GetManager()
        {
            return shaderTilemapManager;
        }
    }

}

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TileMaps.Type;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tiles.TileMap
{
    public class PrimaryShaderTilemap : MonoBehaviour
    {
        private readonly HashSet<Vector2Int> tiles = new HashSet<Vector2Int>(1024);
        private ShaderTilemapManager shaderTilemapManager;
        public ShaderTilemapManager Manager => shaderTilemapManager;

        public void Start()
        {
            shaderTilemapManager = new ShaderTilemapManager(transform, -0.1f, false, TileMapType.Block, 5);
            Grid grid = gameObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(Global.TILE_SIZE, Global.TILE_SIZE, 1f);
        }
        

        public Tilemap GetTilemapForPlacement(Vector2Int cellPosition, [NotNull] Material material)
        {
            tiles.Add(cellPosition);
            return shaderTilemapManager.GetTileMap(material);
        }

        public Tilemap GetTilemap([NotNull] Material material)
        {
            return shaderTilemapManager.GetTileMap(material);
        }

        public void RemoveTile(Vector2Int cellPosition)
        {
            if (!tiles.Contains(cellPosition)) return;
            Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
            shaderTilemapManager.ClearAllOnTile(ref vector3Int);
        }

        public void UnloadPartition(Vector2Int partitionPosition)
        {
            Vector2Int partitionCellPosition = partitionPosition * Global.CHUNK_PARTITION_SIZE;
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                    RemoveTile(partitionCellPosition + new Vector2Int(x,y));
                }
            }
        }
    }
}

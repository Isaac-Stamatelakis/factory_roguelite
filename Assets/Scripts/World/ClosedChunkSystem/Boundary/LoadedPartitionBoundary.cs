using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Chunks.Systems {
    /// <summary>
    /// Creates a boundary around loaded partitions which prevents entities and players from passing through
    /// </summary>
    public class LoadedPartitionBoundary : MonoBehaviour
    {
        private Tilemap mTileMap;
        [SerializeField] private TileBase tile;
        private List<Vector3Int> directions;
        private HashSet<Vector3Int> partitions = new HashSet<Vector3Int>(512);
        public void Initialize()
        {
#pragma warning disable 0162 
            mTileMap = gameObject.AddComponent<Tilemap>();
            if (Global.ShowSystemParameter) {
                gameObject.AddComponent<TilemapRenderer>();
            }
#pragma warning restore 0162 
            gameObject.layer = LayerMask.NameToLayer("Block");
            TilemapCollider2D tilemapCollider2D = gameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider2D.maximumTileChangeCount = 10000000;
            Grid grid = gameObject.AddComponent<Grid>();
            int realPartitionSize = Global.CHUNK_PARTITION_SIZE/2;
            grid.cellSize = new Vector3(realPartitionSize,realPartitionSize,1);
            directions = new List<Vector3Int>{
                Vector3Int.down,
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.up
            };
        }
        public void PartitionLoaded(Vector2Int position) {
            Vector3Int vec3 = (Vector3Int) position;
            mTileMap.SetTile(vec3,null);
            partitions.Add(vec3);
            foreach (Vector3Int direction in directions) {
                OnLoadCheck(vec3 + direction);
            }
        }

        private void OnLoadCheck(Vector3Int position) {
            bool partitionLoaded = partitions.Contains(position);
            if (!partitionLoaded) {
               mTileMap.SetTile(position,tile); 
            }
        }

        public void PartitionUnloaded(Vector2Int position) {
            Vector3Int vec3 = (Vector3Int) position;
            if (HasAdjacentPartition(vec3)) {
                mTileMap.SetTile(vec3,tile);
            } 
            if (partitions.Contains(vec3)) {
                partitions.Remove(vec3);
                foreach (Vector3Int direction in directions) {
                    OnUnloadCheck(vec3 + direction);
                }
            }
            
            
        }
        private bool HasAdjacentPartition(Vector3Int position) {
            foreach (Vector3Int direction in directions) {
                bool hasPartition = partitions.Contains(position+direction);
                if (hasPartition) {
                    return true;
                }
            }
            return false;

        }
        private void OnUnloadCheck(Vector3Int position) {
            if (!HasAdjacentPartition(position)) {
                mTileMap.SetTile(position,null); 
            }
        }

        public void Reset() {
            partitions.Clear();
            mTileMap.ClearAllTiles();
        }
        
    }
}


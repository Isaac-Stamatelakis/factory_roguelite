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
        private TileBase tile;
        List<Vector3Int> directions;
        private HashSet<Vector3Int> partitions = new HashSet<Vector3Int>();
        public void Start() {
            mTileMap = gameObject.AddComponent<Tilemap>();
            if (Global.ShowSystemParameter) {
                gameObject.AddComponent<TilemapRenderer>();
            }
            gameObject.layer = LayerMask.NameToLayer("Block");
            TilemapCollider2D tilemapCollider2D = gameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider2D.maximumTileChangeCount = 10000000;
            Grid grid = gameObject.AddComponent<Grid>();
            int realPartitionSize = Global.ChunkPartitionSize/2;
            grid.cellSize = new Vector3(realPartitionSize,realPartitionSize,1);
            tile = Resources.Load<Tile>("Tiles/Boundary/T~Boundary");
            directions = new List<Vector3Int>{
                Vector3Int.down,
                Vector3Int.left,
                Vector3Int.right,
                Vector3Int.up
            };
        }

        public void partitionLoaded(Vector2Int position) {
            Vector3Int vec3 = (Vector3Int) position;
            mTileMap.SetTile(vec3,null);
            partitions.Add(vec3);
            foreach (Vector3Int direction in directions) {
                onLoadCheck(vec3 + direction);
            }
        }

        private void onLoadCheck(Vector3Int position) {
            bool partitionLoaded = partitions.Contains(position);
            if (!partitionLoaded) {
               mTileMap.SetTile(position,tile); 
            }
        }

        public void partitionUnloaded(Vector2Int position) {
            Vector3Int vec3 = (Vector3Int) position;
            if (hasAdjacentPartition(vec3)) {
                mTileMap.SetTile(vec3,tile);
            } 
            if (partitions.Contains(vec3)) {
                partitions.Remove(vec3);
                foreach (Vector3Int direction in directions) {
                    onUnloadCheck(vec3 + direction);
                }
            }
            
            
        }
        private bool hasAdjacentPartition(Vector3Int position) {
            foreach (Vector3Int direction in directions) {
                bool hasPartition = partitions.Contains(position+direction);
                if (hasPartition) {
                    return true;
                }
            }
            return false;

        }
        private void onUnloadCheck(Vector3Int position) {
            if (!hasAdjacentPartition(position)) {
                mTileMap.SetTile(position,null); 
            }
            
        }
        
    }
}


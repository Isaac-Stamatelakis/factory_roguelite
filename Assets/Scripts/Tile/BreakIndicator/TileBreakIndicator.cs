using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkModule.ClosedChunkSystemModule;
using UnityEngine.Tilemaps;

namespace Tiles {
    public class TileBreakIndicator : MonoBehaviour
    {
        private ClosedChunkSystem closedChunkSystem;
        private Tilemap tilemap;
        private TilemapRenderer tilemapRenderer;
        private Grid grid;
        private Tile[] breakTiles;
        public void init(ClosedChunkSystem closedChunkSystem) {
            this.closedChunkSystem = closedChunkSystem;
            tilemap = gameObject.AddComponent<Tilemap>();
            tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
            grid = gameObject.AddComponent<Grid>();
            grid.cellSize = new Vector3(0.5f,0.5f,1);
            breakTiles = Resources.LoadAll<Tile>("Tiles/BreakTiles");
        }


        public void setBreak(float breakRatio, Vector2Int cellPosition) {
            int index = ((int)(breakRatio * breakTiles.Length)) - 1;
            if (index < 0 || index >= breakTiles.Length) {
                return;
            }
            
            Vector3Int cellPositionVec3 = (Vector3Int) cellPosition;
            tilemap.SetTile(cellPositionVec3,breakTiles[index]);
            Matrix4x4 matrix4X4 = tilemap.GetTransformMatrix(cellPositionVec3);
            if (matrix4X4.isIdentity) {
                tilemap.SetTransformMatrix(cellPositionVec3,getTransformAtPosition(cellPosition));
            } 
        }

        public void removeBreak(Vector2Int cellPosition) {
            tilemap.SetTile((Vector3Int) cellPosition,null);
        }

        public void unloadPartition(Vector2Int partitionPosition) {
            Vector2Int partitionCellPosition = partitionPosition * Global.ChunkPartitionSize;
            for (int x = 0; x < Global.ChunkPartitionSize; x++) {
                for (int y = 0; y < Global.ChunkPartitionSize; y++) {
                    removeBreak(partitionCellPosition + new Vector2Int(x,y));
                }
            }
        }

        private Matrix4x4 getTransformAtPosition(Vector2Int cellPosition) {
            // Calculates random seed based on cellPosition
            System.Random random = new System.Random(cellPosition.x * 73856093 ^ cellPosition.y * 19349663);

            int rotation = random.Next(0, 4) * 90;
            bool mirror = random.Next(0, 2) == 1;
            Quaternion quaternion = Quaternion.Euler(0, 0, rotation);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(quaternion);
            if (mirror) {
                rotationMatrix.m00 *= -1;
            }
            return rotationMatrix;
        }
    }
}

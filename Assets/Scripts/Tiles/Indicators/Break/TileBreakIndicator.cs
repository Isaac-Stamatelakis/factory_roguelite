using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks.Systems;
using TileMaps.Place;
using Tiles.Indicators.Break;
using UnityEngine.Tilemaps;

namespace Tiles {
    public class TileBreakIndicator : MonoBehaviour
    {
        enum BreakHammerTileState
        {
            Slab = HammerTileState.Slab,
            Slant = HammerTileState.Slant,
            Stair = HammerTileState.Stair,
        }
        [SerializeField] private BreakIndicatorStateTile blockTile;
        [SerializeField] private BreakIndicatorStateTile slantTile;
        [SerializeField] private BreakIndicatorStateTile slabTile;
        private Tilemap tilemap;
        private System.Random random;
        public void Start() {
            tilemap = GetComponent<Tilemap>();
            random = new System.Random();
        }


        public void SetBreak(float breakRatio, Vector2Int cellPosition, TileItem tileItem, BaseTileData baseTileData) {
            
            if (tileItem.tile is HammerTile hammerTile)
            {
                HammerTileState? optionalHammerTileValue = hammerTile.GetHammerTileState(baseTileData.state);
                
                if (!optionalHammerTileValue.HasValue) return;
                Debug.Log(optionalHammerTileValue.Value);
                HammerTileState hammerTileState = optionalHammerTileValue.Value;
                if (hammerTileState != HammerTileState.Solid)
                {
                    PlaceHammerTileBreak((BreakHammerTileState)hammerTileState, breakRatio, cellPosition, baseTileData);
                    return;
                }
            }

            TileBase breakTile = blockTile.GetTileAtBreakPercent(breakRatio);
            Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
            bool empty = !tilemap.HasTile(vector3Int);
            tilemap.SetTile(vector3Int,breakTile);
            if (!empty) return;
            int rotation = random.Next(0, 4) * 90;
            PlaceTile.SetTileMapMatrix(tilemap,vector3Int,rotation,false);
        }

        private void PlaceHammerTileBreak(BreakHammerTileState hammerTileState, float breakRatio, Vector2Int cellPosition, BaseTileData baseTileData)
        {
            BreakIndicatorStateTile breakStateTile = GetBreakTile(hammerTileState);
            Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
            TileBase breakTile = breakStateTile.GetTileAtBreakPercent(breakRatio);
            tilemap.SetTile(vector3Int,breakTile);
            PlaceTile.SetTileMapMatrix(tilemap,vector3Int,baseTileData.rotation,baseTileData.mirror);
        }
        

        private BreakIndicatorStateTile GetBreakTile(BreakHammerTileState hammerTileState)
        {
            switch (hammerTileState)
            {
                case BreakHammerTileState.Slab:
                    return slabTile;
                case BreakHammerTileState.Slant:
                case BreakHammerTileState.Stair:
                    return slantTile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hammerTileState), hammerTileState, null);
            }
        }

        public void RotateTile(Vector2Int cellPosition, int direction)
        {
            Vector3Int vector3Int = new Vector3Int(cellPosition.x, cellPosition.y, 0);
            Matrix4x4 matrix4X4 = tilemap.GetTransformMatrix(vector3Int);
            var quaternion = matrix4X4.rotation;
            var newRotation = quaternion * Quaternion.Euler(0, 0, 90*direction);
            matrix4X4.SetTRS(matrix4X4.GetPosition(), newRotation, Vector3.one);
            tilemap.SetTransformMatrix(vector3Int, matrix4X4);
            
        }

        public void RemoveBreak(Vector2Int cellPosition) {
            tilemap.SetTile((Vector3Int) cellPosition,null);
        }

        public void UnloadPartition(Vector2Int partitionPosition) {
            Vector2Int partitionCellPosition = partitionPosition * Global.CHUNK_PARTITION_SIZE;
            for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                    RemoveBreak(partitionCellPosition + new Vector2Int(x,y));
                }
            }
        }
    }
}


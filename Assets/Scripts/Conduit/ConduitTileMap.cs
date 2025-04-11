using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Conduits.Systems;
using Chunks.Partitions;
using Conduit.Conduit;
using Conduits;
using TileMaps.Type;
using Items;
using Tiles;
using UnityEngine.Tilemaps;

namespace TileMaps.Conduit {
    public class ConduitTileMap : AbstractIWorldTileMap<ConduitItem>
    {
        private IConduitSystemManager conduitSystemManager;

        public IConduitSystemManager ConduitSystemManager {set => conduitSystemManager = value; get => conduitSystemManager; }

        public override bool DeleteTile(Vector2 position)
        {
            if (conduitSystemManager == null) {
                return false;
            }

            if (!base.DeleteTile(position))
            {
                return false;
            }
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            conduitSystemManager.SetConduit(cellPosition.x,cellPosition.y,null);
            return true;
        }

        public override ItemObject GetItemObject(Vector2Int position)
        {
            IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(position);
            return conduit?.GetConduitItem();
        }

        public override bool BreakAndDropTile(Vector2Int position, bool dropItem)
        {
            if (conduitSystemManager == null) {
                return false;
            }
            Vector3Int cellPosition = new Vector3Int(position.x,position.y,0);
            if (ReferenceEquals(mTileMap.GetTile(cellPosition), null)) return false;
            IChunkPartition partition = GetPartitionAtPosition(position);
            if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                Debug.LogError("Conduit Tile belonged to non conduit tile chunk partition");
                return false;
            }

            if (dropItem)
            {
                IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(position);
                SpawnItemEntity(conduit.GetConduitItem(),1,position);
            }
            
            BreakTile(position);
            Vector2Int positionInPartition = Global.GetPositionInPartition(position);
            WriteTile(partition,positionInPartition,null);
            conduitSystemManager.SetConduit(position.x,position.y,null);
            return true;
        }

        protected override void SetTile(int x, int y, ConduitItem conduitItem)
        {
            var tile = conduitItem.Tile;
            IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(new Vector2Int(x,y));
            if (conduit == null) return;
            
            var stateTile = tile.getTileAtState(conduit.GetActivatedState());
            tilemap.SetTile(new Vector3Int(x,y,0),stateTile);
        }

        public void RefreshTile(int x, int y)
        {
            Vector3Int cellPosition = new Vector3Int(x,y,0);
            bool tilePlaced = tilemap.GetTile(cellPosition);
            if (!tilePlaced) return;
            
            IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(new Vector2Int(x,y));
            if (conduit == null) return;
            
            var conduitItem = conduit.GetConduitItem();
            var stateTile = conduitItem.Tile.getTileAtState(conduit.GetActivatedState());
            tilemap.SetTile(cellPosition,stateTile);
        }

        public override bool HitTile(Vector2 position, bool dropItem)
        {
            return BreakAndDropTile((Vector2Int)mTileMap.WorldToCell(position), dropItem);
        }

        public void DisconnectConduits(Vector2 position)
        {
            if (conduitSystemManager == null) return;
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            cellPosition.z = 0;
            Vector2Int vect2 = (Vector2Int)cellPosition;
            if (ReferenceEquals(mTileMap.GetTile(cellPosition), null)) return;
            IChunkPartition partition = GetPartitionAtPosition(vect2);
            if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                Debug.LogError("Conduit Tile belonged to non conduit tile chunk partition");
                return;
            }
            IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(vect2);
            if (conduit == null) return;
            
            Vector2 mouseOffset = position - (Vector2)tilemap.CellToWorld(cellPosition);
            float CELL_SIZE = 0.5f;
            int xDirection = GetAdjacentDirection(mouseOffset.x, CELL_SIZE-mouseOffset.y, conduitSystemManager.GetConduitType());
           
            
            if (xDirection != 0)
            {
                Vector2Int xAdjacentPosition = vect2 + new Vector2Int(xDirection, 0);
                ConduitDirectionState xDirectionState = xDirection == 1 ? ConduitDirectionState.Right : ConduitDirectionState.Left;
                if (TryUpdateConduitDirectionState(conduit, vect2, xAdjacentPosition, xDirectionState)) return;
            }
            // Multiply by -1 opposite adjacent direction than x
            int yDirection = -GetAdjacentDirection(CELL_SIZE-mouseOffset.y, mouseOffset.x, conduitSystemManager.GetConduitType());
            if (yDirection != 0)
            {
                Vector2Int yAdjacentPosition = vect2 + new Vector2Int(0, yDirection);
                ConduitDirectionState yDirectionState = yDirection == 1 ? ConduitDirectionState.Up : ConduitDirectionState.Down;
                TryUpdateConduitDirectionState(conduit, vect2, yAdjacentPosition, yDirectionState);
            }
        }

        private bool TryUpdateConduitDirectionState(IConduit conduit, Vector2Int position, Vector2Int adjacentPosition,
            ConduitDirectionState direction)
        {
            IConduit adjConduit = conduitSystemManager.GetConduitAtCellPosition(adjacentPosition);
            if (adjConduit == null) return false;
            
            var reverse = ConduitUtils.Reverse(direction);
            if (conduit.ConnectsDirection(direction))
            {
                conduit.RemoveStateDirection(direction);
                if (adjConduit.ConnectsDirection(reverse)) adjConduit.RemoveStateDirection(reverse);
                conduitSystemManager.ConduitDisconnectUpdate(conduit, adjConduit);
            }
            else
            {
                conduit.AddStateDirection(direction);
                if (!adjConduit.ConnectsDirection(reverse)) adjConduit.AddStateDirection(reverse);
                conduitSystemManager.ConduitJoinUpdate(conduit,adjConduit);
            }

            RefreshTile(position.x,position.y);
            RefreshTile(adjacentPosition.x,adjacentPosition.y);
            return true;
        }
        

        private int GetAdjacentDirection(float offset, float boundaryPos, ConduitType conduitType)
        {
            float CELL_SIZE = 0.5f;
            float PIXELS_PER_CELL = 16f;
            int ORIGIN_WIDTH = 4;
            int pixel = (int)(offset/CELL_SIZE * PIXELS_PER_CELL);
            int boundaryPixel = (int)(boundaryPos/CELL_SIZE*PIXELS_PER_CELL);
            Dictionary<ConduitType, int> origins = new Dictionary<ConduitType, int>
            {
                [ConduitType.Matrix] = 2,
                [ConduitType.Energy] = 4,
                [ConduitType.Signal] = 6,
                [ConduitType.Item] = 8,
                [ConduitType.Fluid] = 10
            };
            int leftCorner = origins[conduitType];
            int rightCorner = leftCorner + ORIGIN_WIDTH;
            if (boundaryPixel < leftCorner || boundaryPixel > rightCorner) return 0;
            return pixel >= leftCorner + ORIGIN_WIDTH/2 ? 1 : -1;
        }
        public override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            return new Vector2Int(cellPosition.x,cellPosition.y);
        }

        protected override void WriteTile(IChunkPartition partition, Vector2Int positionInPartition, ConduitItem item)
        {
            if (partition == null) {
                return;
            }
            if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                Debug.LogError("Conduit Tile Map belonged to non conduit tile chunk partition");
                return;
            }
            conduitTileChunkPartition.SetConduitItem(positionInPartition,GetTileMapType().toConduitType(),item);
        }
    }
}



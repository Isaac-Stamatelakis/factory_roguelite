using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Conduits.Systems;
using Chunks.Partitions;
using Conduits;
using TileMaps.Type;
using Items;
using UnityEngine.Tilemaps;

namespace TileMaps.Conduit {
    public class ConduitIWorldTileMap : AbstractIWorldTileMap<ConduitItem>
    {
        private IConduitSystemManager conduitSystemManager;

        public IConduitSystemManager ConduitSystemManager {set => conduitSystemManager = value;}

        public override void deleteTile(Vector2 position)
        {
            
            if (conduitSystemManager == null) {
                return;
            }
            base.deleteTile(position);
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            conduitSystemManager.SetConduit(cellPosition.x,cellPosition.y,null);
        }
        protected override void SetTile(int x, int y, ConduitItem conduitItem)
        {
            var tile = conduitItem.Tile;
            IConduit conduit = conduitSystemManager.GetConduitAtCellPosition(new Vector2Int(x,y));
            if (ReferenceEquals(conduit, null))
            {
                Debug.LogError($"Tried to place conduit which was null in system at position {new Vector2Int(x,y)}.");
                return;
            }
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

        public override void hitTile(Vector2 position)
        {
            if (conduitSystemManager == null) {
                return;
            }
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            cellPosition.z = 0;
            Vector2Int vect = new Vector2Int(cellPosition.x,cellPosition.y);
            if (mTileMap.GetTile(cellPosition) != null) {
                IChunkPartition partition = GetPartitionAtPosition(vect);
                if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                    Debug.LogError("Conduit Tile belonged to non conduit tile chunk partition");
                    return;
                }
                Vector2Int tilePositionInPartition = base.GetTilePositionInPartition(vect);
                ConduitItem conduitItem = conduitTileChunkPartition.getConduitItemAtPosition(tilePositionInPartition,getType().toConduitType());
                SpawnItemEntity(conduitItem,1,vect);
                BreakTile(new Vector2Int(cellPosition.x,cellPosition.y));
                conduitSystemManager.SetConduit(cellPosition.x,cellPosition.y,null);
            }
        }
        protected override Vector2Int GetHitTilePosition(Vector2 position)
        {
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            return new Vector2Int(cellPosition.x,cellPosition.y);
        }

        protected override void WriteTile(IChunkPartition partition, Vector2Int position, ConduitItem item)
        {
            if (partition == null) {
                return;
            }
            if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                Debug.LogError("Conduit Tile Map belonged to non conduit tile chunk partition");
                return;
            }
            conduitTileChunkPartition.setConduitItem(position,getType().toConduitType(),item);
        }
    }
}



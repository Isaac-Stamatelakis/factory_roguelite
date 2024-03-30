using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ConduitModule.ConduitSystemModule;
using ChunkModule.PartitionModule;
using TileMapModule.Type;

namespace TileMapModule.Conduit {
    public class ConduitTileMap : AbstractTileMap<ConduitItem>
    {
        private ConduitSystemManager conduitSystemManager;

        public ConduitSystemManager ConduitSystemManager {set => conduitSystemManager = value;}

        public override void deleteTile(Vector2 position)
        {
            
            if (conduitSystemManager == null) {
                return;
            }
            base.deleteTile(position);
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            conduitSystemManager.setConduit(cellPosition.x,cellPosition.y,null);
        }
        protected override void setTile(int x, int y, ConduitItem conduitItem)
        {
            RuleTile ruleTile = conduitItem.ruleTile;
            tilemap.SetTile(new Vector3Int(x,y,0),ruleTile);
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
                IChunkPartition partition = getPartitionAtPosition(vect);
                if (partition is not IConduitTileChunkPartition conduitTileChunkPartition) {
                    Debug.LogError("Conduit Tile belonged to non conduit tile chunk partition");
                    return;
                }
                Vector2Int tilePositionInPartition = base.getTilePositionInPartition(vect);
                ConduitItem conduitItem = conduitTileChunkPartition.getConduitItemAtPosition(tilePositionInPartition,getType().toConduitType());
                spawnItemEntity(conduitItem,1,vect);
                breakTile(new Vector2Int(cellPosition.x,cellPosition.y));
                conduitSystemManager.setConduit(cellPosition.x,cellPosition.y,null);
            }
        }
        protected override Vector2Int getHitTilePosition(Vector2 position)
        {
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            return new Vector2Int(cellPosition.x,cellPosition.y);
        }

        protected override void writeTile(IChunkPartition partition, Vector2Int position, ConduitItem item)
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



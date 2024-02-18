using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using ConduitModule.ConduitSystemModule;
using ChunkModule.ClosedChunkSystemModule;
using TileMapModule.Type;
using ConduitModule;
using ConduitModule.Ports;

namespace TileMapModule.Conduit {
    public class ConduitTileMap : AbstractTileMap<ConduitItem, ConduitData>
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
        protected override void setTile(int x, int y, ConduitData conduitData)
        {
            if (conduitData == null || conduitData.getItemObject() == null) {
                return;
            }
            RuleTile ruleTile = ((ConduitItem) conduitData.getItemObject()).ruleTile;
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
            ConduitData placedData = getIdDataInChunk(vect);
            if (mTileMap.GetTile(cellPosition) != null) {
                spawnItemEntity((ConduitItem)placedData.getItemObject(),vect);
                breakTile(new Vector2Int(cellPosition.x,cellPosition.y));
                conduitSystemManager.setConduit(cellPosition.x,cellPosition.y,null);
            }
        }
        protected override Vector2Int getHitTilePosition(Vector2 position)
        {
            Vector3Int cellPosition = mTileMap.WorldToCell(position);
            return new Vector2Int(cellPosition.x,cellPosition.y);
        }

        public override void initPartition(Vector2Int partitionPosition)
        {
            partitions[partitionPosition] = new ConduitData[Global.ChunkPartitionSize,Global.ChunkPartitionSize];
        }
    }
}



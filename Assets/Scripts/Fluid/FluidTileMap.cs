using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapModule;
using ChunkModule.PartitionModule;
using UnityEngine.Tilemaps;

namespace Fluids {
    public class FluidTileMap : AbstractTileMap<FluidTileItem>
    {
        public Dictionary<Vector2Int,int[,]> partitionFills;
        public override void hitTile(Vector2 position)
        {
            
        }

        protected override Vector2Int getHitTilePosition(Vector2 position)
        {
            return Global.getCellPositionFromWorld(position);
        }

        protected override void setTile(int x, int y, FluidTileItem item)
        {
            Tile tile = item.getTile(7);
            tilemap.SetTile(new Vector3Int(x,y,0),tile);
        }

        public int[,] getFill(Vector2Int partitionPosition) {
            return partitionFills[partitionPosition];
        }

        protected override void writeTile(IChunkPartition partition, Vector2Int position, FluidTileItem item)
        {
            throw new System.NotImplementedException();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances {
    public class LadderInstance : TileEntityInstance<Ladder>, IClimableTileEntity, IStaticTileEntity
    {
        public int getSpeed()
        {
            return TileEntityObject.speed;
        }

        public LadderInstance(Ladder tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances {
    public class LadderInstance : TileEntityInstance<Ladder>, IClimableTileEntity, IStaticTileEntity
    {
        public int getSpeed()
        {
            return tileEntity.speed;
        }

        public LadderInstance(Ladder tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }
    }
}


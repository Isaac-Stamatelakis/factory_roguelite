using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances {
    public class DoorInstance : TileEntityInstance<Door>, IRightClickableTileEntity
    {
        public DoorInstance(Door tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            TileEntityUtils.stateIterate(this,2);
        }
    }
}


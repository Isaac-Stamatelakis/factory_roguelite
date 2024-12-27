using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;

namespace TileEntity.Instances {
    public class TrapDoorInstance : TileEntityInstance<TrapDoor>, IRightClickableTileEntity
    {
        
        public TrapDoorInstance(TrapDoor tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onRightClick()
        {
            TileEntityHelper.stateIterate(this,1);
        }
    }
}

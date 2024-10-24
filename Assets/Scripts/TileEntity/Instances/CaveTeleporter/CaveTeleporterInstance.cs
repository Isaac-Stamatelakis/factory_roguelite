using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances {
    public class CaveTeleporterInstance : TileEntityInstance<CaveTeleporter>, IRightClickableTileEntity
    {
        public CaveTeleporterInstance(CaveTeleporter tileEntity, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntity, positionInChunk, tileItem, chunk)
        {
        }

        public void onRightClick()
        {
            tileEntity.uIManager.display<CaveTeleporterInstance,CaveTeleporterUIController>(this);
        }
    }
}


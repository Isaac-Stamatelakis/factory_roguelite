using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule {
    public class StandardTileEntity : TileEntity
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new StandardTileEntityInstance(this, tilePosition,tileItem,chunk);
        }
    }
}

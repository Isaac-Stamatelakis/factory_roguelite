using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances.SimonSays {
    [CreateAssetMenu(fileName = "New Simon Says Tile", menuName = "Tile Entity/SimonSays/ColoredTile")]
    public class SimonSaysColoredTileEntityObject : TileEntityObject
    {
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SimonSaysColoredTileEntityInstance(this,tilePosition,tileItem,chunk);
        }
    }

}

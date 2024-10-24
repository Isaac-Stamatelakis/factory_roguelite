using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances.SimonSays {
    [CreateAssetMenu(fileName = "E~New Simon Says Controller", menuName = "Tile Entity/SimonSays/ColoredTile")]
    public class SimonSaysColoredTileEntity : TileEntity
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new SimonSaysColoredTileEntityInstance(this,tilePosition,tileItem,chunk);
        }
    }

}

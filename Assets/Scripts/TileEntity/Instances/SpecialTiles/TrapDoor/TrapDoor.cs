using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "E~New Trap Door", menuName = "Tile Entity/Trap Door")]
    public class TrapDoor : TileEntity
    {
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new TrapDoorInstance(this,tilePosition,tileItem,chunk);
        }
    }
}


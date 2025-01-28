using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Door", menuName = "Tile Entity/Ladder")]
    public class Ladder : TileEntityObject, IClimableTileEntity
    {
        public int speed;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return null;
        }

        public int GetSpeed()
        {
            return speed;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntityModule.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/CaveTeleporter")]
    public class CaveTeleporter : TileEntity
    {
        public TileEntityUIManager uIManager;
        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CaveTeleporterInstance(this,tilePosition,tileItem,chunk);
        }
    }
}


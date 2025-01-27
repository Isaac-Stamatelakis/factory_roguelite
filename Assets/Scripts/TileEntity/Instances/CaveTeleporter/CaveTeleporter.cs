using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/CaveTeleporter")]
    public class CaveTeleporter : TileEntityObject, IManagedUITileEntity
    {
        public TileEntityUIManager uIManager;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CaveTeleporterInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return uIManager;
        }
    }
}


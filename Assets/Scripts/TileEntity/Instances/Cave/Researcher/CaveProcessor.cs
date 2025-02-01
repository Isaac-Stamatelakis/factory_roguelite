using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/Cave/Processor")]
    public class CaveProcessor : TileEntityObject, IManagedUITileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public TileEntityUIManager uIManager;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CaveProcessorInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return uIManager;
        }
    }
}


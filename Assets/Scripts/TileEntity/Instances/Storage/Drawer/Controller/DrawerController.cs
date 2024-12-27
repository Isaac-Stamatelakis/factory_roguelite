using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using UnityEngine;

namespace TileEntity.Instances.Storage {
    [CreateAssetMenu(fileName ="New Drawer Controller",menuName="Tile Entity/Storage/Drawer/Controller")]
    public class DrawerController : TileEntityObject
    {
        public ConduitPortLayout ConduitLayout;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new DrawerControllerInstance(this,tilePosition,tileItem,chunk);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Conduits.Ports;
using UnityEngine;
using Chunks;
using PlayerModule;
using Items;
using Items.Inventory;
using Entities;

namespace TileEntityModule.Instances.Storage {
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Storage/Drawer/Instance")]
    public class ItemDrawer : TileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public int MaxStacks;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ItemDrawerInstance(this,tilePosition,tileItem,chunk);
        }
    }
}


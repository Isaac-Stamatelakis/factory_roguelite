using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Entities;

namespace TileEntity.Instances
{
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Chest")]
    public class Chest : TileEntityObject, IManagedUITileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public uint Rows;
        public uint Columns;
        public TileEntityUIManager UIManager;

        public override ITileEntityInstance createInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ChestInstance(this,tilePosition,tileItem,chunk);
        }

        public TileEntityUIManager getUIManager()
        {
            return UIManager;
        }
    }
}

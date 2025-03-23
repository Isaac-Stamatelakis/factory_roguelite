using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chunks;
using Conduits.Ports;
using UnityEngine.Tilemaps;
using Items.Inventory;
using Entities;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances
{
    [CreateAssetMenu(fileName ="New Chest",menuName="Tile Entity/Chest")]
    public class Chest : TileEntityObject, IUITileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public uint Rows;
        public uint Columns;
        public AssetReference AssetReference;

        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new ChestInstance(this,tilePosition,tileItem,chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
}

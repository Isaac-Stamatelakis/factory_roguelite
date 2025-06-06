using System.Collections;
using System.Collections.Generic;
using Chunks;
using Conduits.Ports;
using TileEntity.Instances.Caves.VoidMiners;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Void Miner", menuName = "Tile Entity/Cave/Miner")]
    public class VoidMinerObject : TileEntityObject, IUITileEntity
    {
        public AssetReference AssetReference;
        public Tier Tier;
        public ConduitPortLayout ConduitPortLayout;
        public VoidMinerUI VoidMinerUI;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new VoidMinerInstance(this,tilePosition,tileItem,chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
}


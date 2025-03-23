using System.Collections;
using System.Collections.Generic;
using Chunks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/Cave/Teleporter")]
    public class CaveTeleporter : TileEntityObject, IUITileEntity
    {
        public AssetReference AssetReference;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CaveTeleporterInstance(this,tilePosition,tileItem,chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
}


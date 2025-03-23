using Chunks;
using Conduits.Ports;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Caves.Researcher {
    [CreateAssetMenu(fileName = "New Cave Teleporter", menuName = "Tile Entity/Cave/Processor")]
    public class CaveProcessor : TileEntityObject, IUITileEntity
    {
        public ConduitPortLayout ConduitLayout;
        public AssetReference AssetReference;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CaveProcessorInstance(this,tilePosition,tileItem,chunk);
        }
        
        public AssetReference GetUIAssetReference()
        {
            return AssetReference;
        }
    }
}


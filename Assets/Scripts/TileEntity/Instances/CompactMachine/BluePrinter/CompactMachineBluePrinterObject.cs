using Chunks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.CompactMachine.BluePrinter
{
    [CreateAssetMenu(fileName = "E~New Compact Machine", menuName = "Tile Entity/Compact Machine/BluePrinter")]
    public class CompactMachineBluePrinterObject : TileEntityObject, IUITileEntity
    {
        public AssetReference UIAssetReference;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new CompactMachineBluePrinterInstance(this,tilePosition,tileItem,chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return UIAssetReference;
        }
    }
}

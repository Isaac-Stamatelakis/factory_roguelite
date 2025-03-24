using Chunks;
using Robot.Upgrades;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Robot.Upgrader
{
    [CreateAssetMenu(fileName = "E~Signal Source", menuName = "Tile Entity/Robot/Upgrader")]
    public class RobotUpgrader : TileEntityObject, IUITileEntity
    {
        public AssetReference UpgraderUIPrefab;

        private RobotUpgraderInstance instance;
  
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            if (instance == null)
            {
                instance = new RobotUpgraderInstance(this, tilePosition, tileItem, chunk);
            }
            return instance;
        }

        public AssetReference GetUIAssetReference()
        {
            return UpgraderUIPrefab;
        }
    }

    public class RobotUpgraderInstance : TileEntityInstance<RobotUpgrader>
    {
        public RobotUpgraderInstance(RobotUpgrader tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }
    }
}

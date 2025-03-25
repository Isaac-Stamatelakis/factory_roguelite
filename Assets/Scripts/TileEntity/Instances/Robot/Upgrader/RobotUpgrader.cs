using Chunks;
using Player;
using Robot.Upgrades;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TileEntity.Instances.Robot.Upgrader
{
    [CreateAssetMenu(fileName = "E~Signal Source", menuName = "Tile Entity/Robot/Upgrader")]
    public class RobotUpgrader : TileEntityObject, IUITileEntity {
        public AssetReference UpgraderUIPrefab;
        
        private RobotUpgraderInstance instance;
  
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new RobotUpgraderInstance(this, tilePosition, tileItem, chunk);
        }

        public AssetReference GetUIAssetReference()
        {
            return UpgraderUIPrefab;
        }
    }

    public class RobotUpgraderInstance : TileEntityInstance<RobotUpgrader>, ITickableTileEntity, ILoadableTileEntity
    {
        private PlayerRobot playerRobot;
        public RobotUpgraderInstance(RobotUpgrader tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void TickUpdate()
        {
            const float RANGE = 2;
            if (!playerRobot) return;
            float distance = Vector2.Distance(playerRobot.transform.position, GetWorldPosition());
            if (distance > RANGE) return;
            playerRobot.RefreshNanoBots();
        }

        public void Load()
        {
            playerRobot = PlayerManager.Instance.GetPlayer().PlayerRobot;
        }

        public void Unload()
        {
            playerRobot = null;
        }
    }
}

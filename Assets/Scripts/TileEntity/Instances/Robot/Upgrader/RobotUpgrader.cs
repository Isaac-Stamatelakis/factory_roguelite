using Chunks;
using Robot.Upgrades;
using UI;
using UnityEngine;

namespace TileEntity.Instances.Robot.Upgrader
{
    [CreateAssetMenu(fileName = "E~Signal Source", menuName = "Tile Entity/Robot/Upgrader")]
    public class RobotUpgrader : TileEntityObject
    {
        public RobotUpgraderUI UpgraderUIPrefab;
  
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new RobotUpgraderInstance(this, tilePosition, tileItem, chunk);
        }
        
    }

    public class RobotUpgraderInstance : TileEntityInstance<RobotUpgrader>, IRightClickableTileEntity
    {
        public RobotUpgraderInstance(RobotUpgrader tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            RobotUpgraderUI robotUpgradeUI = GameObject.Instantiate(tileEntityObject.UpgraderUIPrefab);
            robotUpgradeUI.DisplayTileEntityInstance(this);
            CanvasController.Instance.DisplayObject(robotUpgradeUI.gameObject);
        }
        
    }
}

using Chunks;
using UI;
using UnityEngine;

namespace TileEntity.Instances.Robot.Upgrader
{
    public class RobotUpgrader : TileEntityObject
    {
        public TileEntityUIManager UIAssetManager;
        public override ITileEntityInstance CreateInstance(Vector2Int tilePosition, TileItem tileItem, IChunk chunk)
        {
            return new RobotUpgraderInstance(this, tilePosition, tileItem, chunk);
        }
        
    }

    public class RobotUpgraderInstance : TileEntityInstance<RobotUpgrader>, IRightClickableTileEntity, IManagedUITileEntity
    {
        public RobotUpgraderInstance(RobotUpgrader tileEntityObject, Vector2Int positionInChunk, TileItem tileItem, IChunk chunk) : base(tileEntityObject, positionInChunk, tileItem, chunk)
        {
        }

        public void OnRightClick()
        {
            tileEntityObject.UIAssetManager.Display<RobotUpgraderInstance,RobotUpgraderUI>(this);
        }

        public TileEntityUIManager getUIManager()
        {
            return tileEntityObject.UIAssetManager;
        }
    }
}

using Player;
using Player.Tool.Object;
using Robot.Tool;
using Robot.Tool.Instances.Gun;
using Robot.Upgrades.LoadOut;
using UnityEngine;

namespace Robot.Tool.Instances
{
    public class LaserGunData : RobotToolData
    {
        
    }
    public class LaserGun : RobotToolInstance<LaserGunData, RobotLaserGunObject>
    {
        public LaserGun(LaserGunData toolData, RobotLaserGunObject robotObject, RobotStatLoadOutCollection statLoadOutCollection, PlayerScript playerScript) : base(toolData, robotObject, statLoadOutCollection, playerScript)
        {
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            return robotObject.ToolIconItem?.getSprite();
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            
        }

        public override string GetModeName()
        {
            return "?";
        }

        public override void Preview(Vector2Int cellPosition)
        {
            
        }

        public override void BeginClickHold(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            
        }

        public override void TerminateClickHold()
        {
           
        }

        public override void ClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey)
        {
            BasicLaserGunProjectile basicLaserGunProjectile = GameObject.Instantiate(robotObject.BasicLaserGunProjectilePrefab);
            basicLaserGunProjectile.transform.position = playerScript.transform.position;
            Vector2 direction = (mousePosition-(Vector2)playerScript.transform.position).normalized;
            basicLaserGunProjectile.Initialize(1f,direction,25f);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            if (time < 0.25f) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }
    }
}

using Player;
using Player.Tool.Object;
using Robot.Tool;
using Robot.Tool.Instances.Gun;
using Robot.Upgrades;
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
            
            if (mouseButtonKey == MouseButtonKey.Right)
            {
                bool explosions = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)LaserGunUpgrade.AoE) > 0;
                if (!explosions) return;
                FireExplosions(mousePosition);
                return;
            }

            if (mouseButtonKey == MouseButtonKey.Left)
            {
                FireLasers(mousePosition);
            }
        }

        private void FireLasers(Vector2 mousePosition)
        {

            const float speed = 30;
            Vector2 direction = (mousePosition - (Vector2)playerScript.transform.position).normalized;
            FireLaser(direction,speed);
            
            int bonusShots = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)LaserGunUpgrade.MultiShot);
            
            const float totalSpreadAngle = 4f;
            float angleBetweenShots = bonusShots > 1 ? totalSpreadAngle / (bonusShots - 1) : 0;
            const float startAngle = -totalSpreadAngle / 2f;
            for (int i = 0; i < bonusShots; i++)
            {
                float randomOffset = UnityEngine.Random.Range(-angleBetweenShots / 2f, angleBetweenShots / 2f);
                float currentAngle = startAngle + angleBetweenShots * (i+1) + randomOffset;
                float angleInRadians = currentAngle * Mathf.Deg2Rad;
                Vector2 spreadDirection = new Vector2(
                    direction.x * Mathf.Cos(angleInRadians) - direction.y * Mathf.Sin(angleInRadians),
                    direction.x * Mathf.Sin(angleInRadians) + direction.y * Mathf.Cos(angleInRadians)
                );
                FireLaser(spreadDirection.normalized,speed + UnityEngine.Random.Range(-5f,5f));
            }
        }

        private void FireLaser(Vector2 direction, float speed)
        {
            BasicLaserGunProjectile basicLaserGunProjectile = GameObject.Instantiate(robotObject.BasicLaserGunProjectilePrefab);
            basicLaserGunProjectile.transform.position = playerScript.transform.position;
            basicLaserGunProjectile.Initialize(1f, direction, speed);
        }

        private void FireExplosions(Vector2 mousePosition)
        {
            Vector2 direction = (mousePosition - (Vector2)playerScript.transform.position).normalized;
            LaserGunExplosionProjectile laserGunExplosionProjectile = GameObject.Instantiate(robotObject.LaserGunExplosionProjectilePrefab);
            laserGunExplosionProjectile.transform.position = playerScript.transform.position;
            laserGunExplosionProjectile.Initialize(1f, direction, 10f);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            const float MAX_FIRE_RATE_UPGRADES = 10;
            const float BASE_FIRE_RATE = 0.33f;
            const float MIN_FIRE_RATE = 0.05f;
            float fireRateUpgrades = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)LaserGunUpgrade.FireRate);
            if (fireRateUpgrades > MAX_FIRE_RATE_UPGRADES) fireRateUpgrades = MAX_FIRE_RATE_UPGRADES;
            float fireRate = Mathf.Lerp(BASE_FIRE_RATE, MIN_FIRE_RATE, fireRateUpgrades/MAX_FIRE_RATE_UPGRADES);
            const float EXPLOSION_RATE_REDUCTION = 4;
            if (mouseButtonKey == MouseButtonKey.Right) fireRate *= EXPLOSION_RATE_REDUCTION;
            if (time < fireRate) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }
    }
}

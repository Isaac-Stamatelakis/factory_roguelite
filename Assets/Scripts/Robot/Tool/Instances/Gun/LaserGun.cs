using System.Collections.Generic;
using Player;
using Player.Tool.Object;
using Robot.Tool;
using Robot.Tool.Instances.Gun;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.LoadOut;
using UnityEngine;

namespace Robot.Tool.Instances
{
    public class LaserGunData : RobotToolData
    {
        
    }

    public class ToolObjectPool
    {
        public ToolObjectPool(int count, GameObject prefab, Transform container, string name)
        {
            GameObject objectContainer = new GameObject(name);
            objectContainer.transform.SetParent(container,false);
            while (count > 0)
            {
                GameObject obj = GameObject.Instantiate(prefab, objectContainer.transform, false);
                pool.Push(obj);
                obj.SetActive(false);
                count--;
            }
        }

        private Stack<GameObject> pool = new Stack<GameObject>();

        public GameObject TakeFromPool()
        {
            if (pool.Count == 0) return null;
            GameObject top = pool.Pop();
            top.SetActive(true);
            return top;
        }
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.position = Vector3.zero;
            pool.Push(obj);
        }
    }
    public class LaserGun : RobotToolInstance<LaserGunData, RobotLaserGunObject>
    {
        private ToolObjectPool bombParticlePool;
        private ToolObjectPool laserParticlePool;
        public LaserGun(LaserGunData toolData, RobotLaserGunObject robotObject, RobotStatLoadOutCollection statLoadOutCollection, PlayerScript playerScript) : base(toolData, robotObject, statLoadOutCollection, playerScript)
        {
            // TODO auto adjust this based on firing rate
            bombParticlePool = new ToolObjectPool(10, robotObject.ExplosionParticlePrefab, playerScript.PersistentObjectContainer, "GunAoE");
            laserParticlePool = new ToolObjectPool(16, robotObject.LaserParticlePrefab, playerScript.PersistentObjectContainer, "Laser");
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

        public override void TerminateClickHold(MouseButtonKey mouseButtonKey)
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
            const float RANDOM_SPEED_RANGE = 5f;
            const float speed = 30;
            Vector2 dif = mousePosition - (Vector2)playerScript.transform.position;
            if (dif == Vector2.zero)
            {
                dif = mousePosition +Vector2.one * 0.05f - (Vector2)playerScript.transform.position;
            }
            Vector2 direction = dif.normalized;
            FireLaser(direction,speed + UnityEngine.Random.Range(-RANDOM_SPEED_RANGE,RANDOM_SPEED_RANGE),true);
            
            int bonusShots = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)LaserGunUpgrade.MultiShot);
            
            const float totalSpreadAngle = 4f;
            float angleBetweenShots = bonusShots > 1 ? totalSpreadAngle / (bonusShots - 1) : totalSpreadAngle;
            const float startAngle =  -totalSpreadAngle / 2f;
            
            int spreadFire = 2 * (bonusShots / 2); // Converted to even number
            for (int i = 0; i < spreadFire; i++)
            {
                float randomOffset = UnityEngine.Random.Range(-angleBetweenShots / 2f, angleBetweenShots / 2f);
                float currentAngle = startAngle + angleBetweenShots * i + randomOffset;
                float angleInRadians = currentAngle * Mathf.Deg2Rad;
                FireLaserAtAngle(direction,angleInRadians,speed + UnityEngine.Random.Range(-RANDOM_SPEED_RANGE,RANDOM_SPEED_RANGE));
            }

            if (bonusShots % 2 == 1)
            {
                float randomAngle = UnityEngine.Random.Range(-totalSpreadAngle / 2f, totalSpreadAngle / 2f);
                float angleInRadians = randomAngle * Mathf.Deg2Rad;
                FireLaserAtAngle(direction,angleInRadians,speed + UnityEngine.Random.Range(-RANDOM_SPEED_RANGE,RANDOM_SPEED_RANGE));
            }
        }

        private void FireLaserAtAngle(Vector2 direction, float angle, float speed)
        {
            Vector2 spreadDirection = new Vector2(
                direction.x * Mathf.Cos(angle) - direction.y * Mathf.Sin(angle),
                direction.x * Mathf.Sin(angle) + direction.y * Mathf.Cos(angle)
            );
            FireLaser(spreadDirection.normalized,speed,false);
        }

        private void FireLaser(Vector2 direction, float speed, bool usePool)
        {
            if (!playerRobot.TryConsumeEnergy(RobotLaserGunUpgradeInfo.COST_PER_LASER,0.1f)) return;
            BasicLaserGunProjectile basicLaserGunProjectile = GameObject.Instantiate(robotObject.BasicLaserGunProjectilePrefab,playerScript.TemporaryObjectContainer,false);
            float z = basicLaserGunProjectile.transform.position.z;
            var vector3 = playerRobot.gunController.GetEdgePosition();
            vector3.z = z;
            basicLaserGunProjectile.transform.position = vector3;
            basicLaserGunProjectile.Initialize(1f, direction, speed, usePool ? laserParticlePool : null);
        }

        private void FireExplosions(Vector2 mousePosition)
        {
            if (!playerRobot.TryConsumeEnergy(RobotLaserGunUpgradeInfo.COST_PER_EXPLOSION,0.1f)) return;
            Vector2 dif = mousePosition - (Vector2)playerScript.transform.position;
            if (dif == Vector2.zero)
            {
                dif = mousePosition +Vector2.one * 0.05f - (Vector2)playerScript.transform.position;
            }
            Vector2 direction = dif.normalized;
            
            LaserGunExplosionProjectile laserGunExplosionProjectile = GameObject.Instantiate(robotObject.LaserGunExplosionProjectilePrefab,playerScript.TemporaryObjectContainer,false);
            float z = laserGunExplosionProjectile.transform.position.z;
            var vector3 = playerRobot.gunController.GetEdgePosition();
            vector3.z = z;
            laserGunExplosionProjectile.transform.position = vector3;
            laserGunExplosionProjectile.Initialize(1f, direction, 10f,bombParticlePool);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, MouseButtonKey mouseButtonKey, float time)
        {
            
            float fireRate = GetFireRate(RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)LaserGunUpgrade.FireRate));
            const float EXPLOSION_RATE_REDUCTION = 4;
            if (mouseButtonKey == MouseButtonKey.Right) fireRate *= EXPLOSION_RATE_REDUCTION;
            if (time < fireRate) return false;
            ClickUpdate(mousePosition, mouseButtonKey);
            return true;
        }

        public static float GetFireRate(float upgrades)
        {
            const float MAX_FIRE_RATE_UPGRADES = 10;
            const float BASE_FIRE_RATE = 0.33f;
            const float MIN_FIRE_RATE = 0.05f;
            if (upgrades > MAX_FIRE_RATE_UPGRADES) upgrades = MAX_FIRE_RATE_UPGRADES;
            return Mathf.Lerp(BASE_FIRE_RATE, MIN_FIRE_RATE, upgrades/MAX_FIRE_RATE_UPGRADES);
        }
    }
}

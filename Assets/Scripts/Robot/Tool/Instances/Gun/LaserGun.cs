using System;
using System.Collections.Generic;
using System.Numerics;
using Items;
using Player;
using Player.Robot;
using Player.Tool;
using Player.Tool.Object;
using Robot.Tool;
using Robot.Tool.Instances.Gun;
using Robot.Upgrades;
using Robot.Upgrades.Info.Instances;
using Robot.Upgrades.LoadOut;
using UnityEngine;
using Random = System.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Robot.Tool.Instances
{
    public enum LaserGunMode
    {
        Light,
        Blast
    }
    
    public class LaserGunData : RobotToolData
    {
        public LaserGunMode LaserGunMode;
    }

    public class ToolObjectPool
    {
        private float growthFactor;
        private int minCount;
        private GameObject prefab;
        private Transform container;
        private System.Random random;
        public ToolObjectPool(int count, GameObject prefab, Transform parentContainer, string name, float growthFactor)
        {
            this.growthFactor = growthFactor;
            random = new Random();
            this.prefab = prefab;
            this.minCount = count;
            GameObject objectContainer = new GameObject(name);
            this.container = objectContainer.transform;
            container.SetParent(parentContainer,false);
            while (count > 0)
            {
                PushToPool();
                count--;
            }
        }

        private void PushToPool()
        {
            GameObject obj = UnityEngine.Object.Instantiate(prefab, container, false);
            pool.Push(obj);
            obj.SetActive(false);
        }
        private Stack<GameObject> pool = new Stack<GameObject>();

        public GameObject TakeFromPool()
        {
            if (pool.Count < minCount)
            {
                double r = random.NextDouble();
                if (r < 2*growthFactor)
                {
                    PushToPool();
                }
            }

            if (pool.Count == 0) return null;
            
            GameObject top = pool.Pop();
            top.SetActive(true);
            return top;
        }
        public void ReturnToPool(GameObject obj)
        {
            if (1 + pool.Count > minCount)
            {
                double r = random.NextDouble();
                if (r < growthFactor)
                {
                    GameObject.Destroy(obj);
                    return;
                }
            }
            obj.SetActive(false);
            obj.transform.position = Vector3.zero;
            pool.Push(obj);
        }
    }
    public class LaserGun : RobotToolInstance<LaserGunData, RobotLaserGunObject>, IMultiSpriteTool
    {
        const float MAX_FIRE_RATE_UPGRADES = 10;
        const float BASE_FIRE_RATE = 0.417f;
        const float MIN_FIRE_RATE = 0.05f;   
        const float EXPLOSION_RATE_REDUCTION = 4;
        private ToolObjectPool bombParticlePool;
        private ToolObjectPool laserParticlePool;
        private Rigidbody2D playerRb;
        public LaserGun(LaserGunData toolData, RobotLaserGunObject robotObject, RobotStatLoadOutCollection statLoadOutCollection, PlayerScript playerScript) : base(toolData, robotObject, statLoadOutCollection, playerScript)
        {
            bombParticlePool = new ToolObjectPool(10, robotObject.ExplosionParticlePrefab, playerScript.PersistentObjectContainer, "GunAoE",0.1f);
            laserParticlePool = new ToolObjectPool(16, robotObject.LaserParticlePrefab, playerScript.PersistentObjectContainer, "Laser",0.01f);
            playerRb = playerScript.GetComponent<Rigidbody2D>();
        }
        
        public override Sprite GetPrimaryModeSprite()
        {
            return robotObject.ToolIconItem?.GetSprite();
        }

        public override void ModeSwitch(MoveDirection moveDirection, bool subMode)
        {
            if (RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)LaserGunUpgrade.AoE) == 0)
            {
                toolData.LaserGunMode = LaserGunMode.Light;
                return;
            }
            switch (toolData.LaserGunMode)
            {
                case LaserGunMode.Light:
                    toolData.LaserGunMode = LaserGunMode.Blast;
                    break;
                case LaserGunMode.Blast:
                    toolData.LaserGunMode = LaserGunMode.Light;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetModeName()
        {
            return toolData.LaserGunMode.ToString();
        }

        public override void Preview(Vector2Int cellPosition, bool autoSelectOn)
        {
            
        }

        public override RobotArmState GetRobotArmAnimation()
        {
            switch (toolData.LaserGunMode)
            {
                case LaserGunMode.Light:
                    return RobotArmState.LaserGun;
                case LaserGunMode.Blast:
                    return RobotArmState.LaserExplosion;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetSubState()
        {
            return (int)toolData.LaserGunMode;
        }

        public override void BeginClickHold(Vector2 mousePosition)
        {
            
        }

        public override void TerminateClickHold()
        {
           
        }

        public override void ClickUpdate(Vector2 mousePosition)
        {
            Fire(mousePosition);
        }

        private void Fire(Vector2 mousePosition)
        {
            switch (toolData.LaserGunMode)
            {
                case LaserGunMode.Light:
                    FireLasers(mousePosition);
                    break;
                case LaserGunMode.Blast:
                    bool explosions = RobotUpgradeUtils.GetDiscreteValue(statLoadOutCollection, (int)LaserGunUpgrade.AoE) > 0;
                    if (!explosions) return;
                    FireExplosions(mousePosition);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            playerRobot.gunController.SyncAnimation(toolData.LaserGunMode == LaserGunMode.Light ? RobotArmState.LaserGun : RobotArmState.LaserExplosion,0);

            float laserKnockback = RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)LaserGunUpgrade.Knockback);
            if (laserKnockback == 0) return;
            if (toolData.LaserGunMode == LaserGunMode.Blast)
            {
                laserKnockback *= 8;
            }
            
            Vector2 mouseDirection = ((Vector2)playerRobot.transform.position - mousePosition).normalized;
            playerRb.AddForce(laserKnockback*mouseDirection,ForceMode2D.Impulse);
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
            var vector3 = playerRobot.gunController.GetEdgePosition(1f);
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
            var vector3 = playerRobot.gunController.GetEdgePosition(1f);
            vector3.z = z;
            laserGunExplosionProjectile.transform.position = vector3;
            laserGunExplosionProjectile.Initialize(1f, direction, 10f,bombParticlePool);
        }

        public override bool HoldClickUpdate(Vector2 mousePosition, float time)
        {
            float fireRate = GetFireRate(RobotUpgradeUtils.GetContinuousValue(statLoadOutCollection, (int)LaserGunUpgrade.FireRate),toolData.LaserGunMode);
            if (time < fireRate) return false;
            Fire(mousePosition);
            return true;
        }

        public static float GetFireRate(float upgrades, LaserGunMode mode)
        {
            if (upgrades > MAX_FIRE_RATE_UPGRADES) upgrades = MAX_FIRE_RATE_UPGRADES;
            float fireRate = Mathf.Lerp(BASE_FIRE_RATE, MIN_FIRE_RATE, upgrades/MAX_FIRE_RATE_UPGRADES);
            if (mode == LaserGunMode.Blast) fireRate *= EXPLOSION_RATE_REDUCTION;
            return fireRate;
        }

        public static float GetAnimationSpeed(float upgrades,LaserGunMode mode)
        {
            float fireRate = GetFireRate(upgrades,mode);
            const float ANIMATION_SPEED = BASE_FIRE_RATE;
            return ANIMATION_SPEED / fireRate;
        }
        

        public ItemObject GetDisplayItem()
        {
            switch (toolData.LaserGunMode)
            {
                case LaserGunMode.Light:
                    return robotObject.ToolIconItem;
                case LaserGunMode.Blast:
                    return robotObject.HeavyBlastIcon;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

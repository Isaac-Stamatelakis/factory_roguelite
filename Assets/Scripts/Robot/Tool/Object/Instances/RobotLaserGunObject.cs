using Items;
using Robot.Tool.Instances.Drill;
using Robot.Tool.Instances.Gun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Robots/Tools/Gun")]
    public class RobotLaserGunObject : RobotToolObject
    {
        public ItemObject HeavyBlastIcon;
        public BasicLaserGunProjectile BasicLaserGunProjectilePrefab;
        public LaserGunExplosionProjectile LaserGunExplosionProjectilePrefab;
        public GameObject ExplosionParticlePrefab;
        public GameObject LaserParticlePrefab;
    }
}

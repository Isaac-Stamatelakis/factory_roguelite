using Robot.Tool.Instances.Drill;
using Robot.Tool.Instances.Gun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Tool.Object
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "Robots/Tools/Gun")]
    public class RobotLaserGunObject : RobotToolObject
    {
        public BasicLaserGunProjectile BasicLaserGunProjectilePrefab;
        public LaserGunExplosionProjectile LaserGunExplosionProjectilePrefab;
    }
}

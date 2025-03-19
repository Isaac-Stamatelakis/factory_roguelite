using Entities.Mobs;
using Robot.Tool.Instances.Gun;
using UnityEngine;

namespace Entities.Mob.Display
{
    public class WormBodyPart : MonoBehaviour, IDamageableEntity
    {
        public void Damage(float damage, Vector2 damageDirection)
        {
            GetComponentInParent<MobEntity>().Damage(damage, damageDirection);
        }
    }
}

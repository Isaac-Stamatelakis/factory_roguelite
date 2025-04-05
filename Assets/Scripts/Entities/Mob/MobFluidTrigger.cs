using System;
using Entities.Mobs;
using Fluids;
using Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Mob
{
    public class MobFluidTrigger : MonoBehaviour
    {
        [SerializeField] private bool damagedByFluid = true;
        private MobEntity mobEntity;
        private FluidTileItem collidingFluid;
        private float damageCounter;
        private float fluidDamage;

        public void Start()
        {
            mobEntity = GetComponent<MobEntity>();
        }

        public void FixedUpdate()
        {
            if (fluidDamage <= 0) return;
            damageCounter += Time.fixedDeltaTime;
            if (damageCounter < 1) return;
            mobEntity.Damage(fluidDamage,Vector2.zero);
            damageCounter = 0;
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z += 2;
                transform.localPosition = vector3;
                if (!damagedByFluid) return;
                Vector2 bottomPosition = (Vector2)transform.position + Vector2.down * GetComponent<SpriteRenderer>().bounds.extents.y;
                Vector2 collisionPoint = other.ClosestPoint(bottomPosition);
                FluidWorldTileMap fluidWorldTileMap = other.GetComponent<FluidWorldTileMap>();
                fluidWorldTileMap ??= other.GetComponentInParent<FluidWorldTileMap>();
                if (!fluidWorldTileMap)
                {
                    collidingFluid = null;
                    fluidDamage = 0;
                    return;
                }
                collidingFluid = fluidWorldTileMap.GetFluidItem(collisionPoint);
                if (!collidingFluid) return;
                if (collidingFluid.fluidOptions.DamagePerSecond <= 0.05f)
                {
                    fluidDamage = 0;
                    return;
                }
                fluidDamage = collidingFluid.fluidOptions.DamagePerSecond;
                mobEntity.Damage(fluidDamage,Vector2.zero);
                damageCounter = 0;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z -= 2;
                transform.localPosition = vector3;
            }
        }
    }
}

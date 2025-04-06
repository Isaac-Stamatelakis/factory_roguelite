using System;
using Entities.Mob.Movement;
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
        [SerializeField] private bool drowns = true;
        private MobEntity mobEntity;
        private bool inFluid;
        private FluidTileItem collidingFluid;
        private float fluidDamage;
        private float fluidCounter;
        private const float DROWN_TIME = 10;
        private const float DROWN_DAMAGE = 1f;
        private uint drownCounter;
        private uint damageCounter;
        private IEnterFluidEntityMovement fluidEntityMovement;

        public void Start()
        {
            mobEntity = GetComponent<MobEntity>();
            fluidEntityMovement = GetComponent<IEnterFluidEntityMovement>();
        }

        public void FixedUpdate()
        {
            if (!collidingFluid) return;
            if (fluidDamage > 0) damageCounter++;
            fluidCounter += Time.fixedDeltaTime;
            if (drowns && fluidCounter > DROWN_TIME)
            {
                drownCounter++;
                if (drownCounter >= 50)
                {
                    drownCounter = 0;
                    mobEntity.Damage(DROWN_DAMAGE,Vector2.zero);
                }
            }
            if ( damageCounter < 50) return;
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
                fluidCounter = 0;
                
                Vector2 bottomPosition = (Vector2)transform.position + Vector2.down * GetComponent<SpriteRenderer>().bounds.extents.y;
                Vector2 collisionPoint = other.ClosestPoint(bottomPosition);
                FluidWorldTileMap fluidWorldTileMap = other.GetComponent<FluidWorldTileMap>();
                fluidWorldTileMap ??= other.GetComponentInParent<FluidWorldTileMap>();
                if (!fluidWorldTileMap)
                {
                    collidingFluid = null;
                    collidingFluid = null;
                    fluidDamage = 0;
                    return;
                }
                collidingFluid = fluidWorldTileMap.GetFluidItem(collisionPoint);
                fluidEntityMovement?.OnEnterFluid(collisionPoint);
                if (!collidingFluid || !damagedByFluid) return;
                if (collidingFluid.fluidOptions.DamagePerSecond <= 0.05f)
                {
                    fluidDamage = 0;
                    return;
                }
                fluidDamage = collidingFluid.fluidOptions.DamagePerSecond;
                mobEntity.Damage(fluidDamage,Vector2.zero);
                drownCounter = 0;
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                collidingFluid = null;
                var vector3 = transform.localPosition;
                vector3.z -= 2;
                transform.localPosition = vector3;
                fluidEntityMovement?.OnExitFluid();
            }
        }
    }
}

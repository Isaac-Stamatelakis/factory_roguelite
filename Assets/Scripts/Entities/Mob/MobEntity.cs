using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Entities;
using Robot.Tool.Instances.Gun;

namespace Entities.Mobs {
    public enum MobSpawnCondition
    {
        OnGround,
        InAir,
        None
    }
    public class MobEntity : Entity, ISerializableEntity, IDamageableEntity
    {
        public MobSpawnCondition MobSpawnCondition;
        public bool TakesKnockback = true;
        public bool RayCastUnLoadable = true;
        private string id;
        public float Health = 10;
        public void Deseralize(SerializedMobEntityData entityData) {
            this.id = entityData.Id;
            if (entityData.Health > float.MinValue)
            {
                this.Health = entityData.Health;
            }
            
        }

        public void Damage(float amount, Vector2 damageDirection)
        {
            Health -= amount;
            if (Health <= 0)
            {
                GameObject.Destroy(gameObject);
            }

            StartCoroutine(DamageRedEffect());
            if (!TakesKnockback) return;
            GetComponent<Rigidbody2D>().AddForce(damageDirection * 2, ForceMode2D.Impulse);
        }

        private IEnumerator DamageRedEffect()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (!renderer) yield break;
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            renderer.color = Color.white;
        }

        public override void initalize()
        {
            
        }

        public SeralizedEntityData serialize() {
            SerializedMobEntityData serializedMobData = new SerializedMobEntityData{
                Id = id,
                Health = Health
            };
            return new SeralizedEntityData(
                type: EntityType.Mob,
                position: transform.position,
                data: JsonConvert.SerializeObject(serializedMobData)
            );
        }
    }

}

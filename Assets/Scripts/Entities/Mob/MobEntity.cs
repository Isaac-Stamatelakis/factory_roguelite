using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Entities;
using Items;
using Robot.Tool.Instances.Gun;

namespace Entities.Mobs {
    public enum MobSpawnCondition
    {
        OnGround,
        InAir,
        None
    }

    public enum MobLootSpawnPlacement
    {
        OnSelf,
        OnFirstChild
    }
    public class MobEntity : Entity, ISerializableEntity, IDamageableEntity
    {
        public MobSpawnCondition MobSpawnCondition = MobSpawnCondition.OnGround;
        public MobLootSpawnPlacement MobLootSpawnPlacement = MobLootSpawnPlacement.OnSelf;
        public bool TakesKnockback = true;
        public bool RayCastUnLoadable = true;
        public float Health = 10;
        public LootTable LootTable;
        
        private string id;
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
                if (transform.parent)
                {
                    ItemEntityFactory.SpawnLootTable(GetLootSpawnPosition(),LootTable,transform.parent);
                }
                GameObject.Destroy(gameObject);
                return;
            }
            StartCoroutine(DamageRedEffect());
            if (!TakesKnockback) return;
            GetComponent<Rigidbody2D>().AddForce(damageDirection * 2, ForceMode2D.Impulse);
        }

        private Vector2 GetLootSpawnPosition()
        {
            switch (MobLootSpawnPlacement)
            {
                case MobLootSpawnPlacement.OnSelf:
                    return transform.position;
                case MobLootSpawnPlacement.OnFirstChild:
                    return transform.childCount == 0 ? transform.position : transform.GetChild(0).position;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z = 2;
                transform.localPosition = vector3;
            }
        }
        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Fluid")
            {
                var vector3 = transform.localPosition;
                vector3.z = 0;
                transform.localPosition = vector3;
            }
        }
    }

}

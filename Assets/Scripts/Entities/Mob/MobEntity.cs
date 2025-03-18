using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Entities;

namespace Entities.Mobs {
    public enum MobSpawnCondition
    {
        OnGround,
        InAir,
        None
    }
    public class MobEntity : Entity, ISerializableEntity
    {
        public MobSpawnCondition MobSpawnCondition;
        private string id;
        public float Health = 10;
        public void Deseralize(SerializedMobEntityData entityData) {
            this.id = entityData.Id;
            if (entityData.Health > float.MinValue)
            {
                this.Health = entityData.Health;
            }
            
        }

        public void Damage(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                GameObject.Destroy(gameObject);
            }
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

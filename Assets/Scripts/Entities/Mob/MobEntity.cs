using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Entities;
using Entities.Mob;
using Items;
using Robot.Tool.Instances.Gun;

namespace Entities.Mobs {
    public enum MobSpawnCondition
    {
        OnGround = 0,
        InAir = 1,
        None = 2,
        InWater = 3
    }

    public enum MobLootSpawnPlacement
    {
        OnSelf,
        OnFirstChild
    }

    public enum SerializableMobComponentType
    {
        CrystalCrawler = 0 
    }

    public interface ISerializableMobComponent
    {
        public SerializableMobComponentType ComponentType { get; }
        public string Serialize();
        public void Deserialize(string data);
    }

    public interface ICaveInitiazableMobComponent : ISerializableMobComponent
    {
        public string Initialize();
    }
    public class MobEntity : Entity, ISerializableEntity, IDamageableEntity
    {
        public enum MobDeathParticles
        {
            Standard = 0,
            None = 1,
            
        }
        public MobSpawnCondition MobSpawnCondition = MobSpawnCondition.OnGround;
        public MobLootSpawnPlacement MobLootSpawnPlacement = MobLootSpawnPlacement.OnSelf;
        public bool TakesKnockback = true;
        public bool RayCastUnLoadable = true;
        public float Health = 10;
        public LootTable LootTable;
        public MobDeathParticles DeathParticles = MobDeathParticles.None;
        
        private string id;
        public void Deserialize(SerializedMobEntityData entityData) {
            this.id = entityData.Id;
            if (entityData.Health > float.MinValue)
            {
                this.Health = entityData.Health;
            }
            
        }

        public void Damage(float amount, Vector2 damageDirection)
        {
            bool dead = Health <= float.MinValue;
            if (dead) return;
            Health -= amount;
            if (Health <= 0)
            {
                if (transform.parent)
                {
                    ItemEntityFactory.SpawnLootTable(GetLootSpawnPosition(),LootTable,transform.parent);
                }

                if (DeathParticles != MobDeathParticles.None)
                {
                    transform.parent.GetComponent<MobEntityParticleController>().PlayDeathParticles(transform.position, DeathParticles);
                }
                
                Health = float.MinValue;
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
        

        public SeralizedEntityData serialize()
        {
            ISerializableMobComponent[] serializableMobComponents = GetComponents<ISerializableMobComponent>();
            SerializedMobEntityData serializedMobData;
            if (serializableMobComponents.Length == 0)
            {
                serializedMobData = new SerializedMobEntityData(id, Health, null);
            }
            else
            {
                Dictionary<SerializableMobComponentType, string> componentDataDictionary = new();
                foreach (ISerializableMobComponent serializableMobComponent in serializableMobComponents)
                {
                    string componentData = serializableMobComponent.Serialize();
                    componentDataDictionary[serializableMobComponent.ComponentType] =  componentData;
                }
                serializedMobData = new SerializedMobEntityData(id, Health, componentDataDictionary);
            }
            
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

        public override void Initialize()
        {
            
        }
    }

}

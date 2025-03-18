using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Entities;

namespace Entities.Mobs {
    public class MobEntity : Entity, ISerializableEntity
    {
        private string id;
        public void Deseralize(string id, string data) {
            this.id = id;
            if (data == null) {
                return;
            }
            /*
            foreach (KeyValuePair<string,string> kvp in componentData) {
                Type type = Type.GetType(kvp.Key);
                var component = GetComponent(type);
                if (component == null) {
                    Debug.LogError("Type '" + kvp.Key + "' could not be deserialized as not on gameObject");
                    continue;
                }
                if (component is not ISerializableMobComponent serializableMobComponent) {
                    Debug.LogError("Type '" + kvp.Key + "' could not be deserialized as not ISerializedMobComponent");
                    continue;
                }
                serializableMobComponent.deseralize(kvp.Value);
            }
            */
        }

        public override void initalize()
        {
            
        }

        public SeralizedEntityData serialize() {
            ISerializableMobComponent[] serializableMobComponents = GetComponents<ISerializableMobComponent>();
            
            SerializedMobEntityData serializedMobData = new SerializedMobEntityData{
                Id = id,
                Data = null
            };
            return new SeralizedEntityData(
                type: EntityType.Mob,
                position: transform.position,
                data: JsonConvert.SerializeObject(serializedMobData)
            );
        }
    }

}

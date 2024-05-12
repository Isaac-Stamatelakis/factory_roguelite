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
        public void deseralize(string id, Dictionary<string,string> componentData) {
            this.id = id;
            if (componentData == null) {
                return;
            }
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
        }

        public override void initalize()
        {
            
        }

        public SeralizedEntityData serialize() {
            ISerializableMobComponent[] serializableMobComponents = GetComponents<ISerializableMobComponent>();
            Dictionary<string, string> componentSerializedDict = serializableMobComponents.Length == 0 
                ? null
                : new Dictionary<string, string>();
            foreach (ISerializableMobComponent component in serializableMobComponents) {
                string typeName = component.GetType().FullName;
                componentSerializedDict.Add(typeName,component.serialize());
            }
            SerializedMobData serializedMobData = new SerializedMobData(
                id,
                componentSerializedDict
            );
            Debug.Log(id);
            return new SeralizedEntityData(
                type: EntityType.Mob,
                position: transform.position,
                data: JsonConvert.SerializeObject(serializedMobData)
            );
        }
    }

}

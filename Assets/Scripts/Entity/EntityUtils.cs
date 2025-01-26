using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Entities.Mobs;
using Item.Slot;
using Newtonsoft.Json;

namespace Entities {
    public static class EntityUtils {
        private static readonly string entityFolderPath = "Assets/Prefabs/Entities";
        public static string EntityFolderPath => entityFolderPath;
        public static string getObjectPath(string id) {
            return $"{entityFolderPath}/{id}";
        }

        public static List<string> getAllIds() {
            string[] fullFolderName = Directory.GetDirectories(entityFolderPath);
            List<string> ids = new List<string>();
            foreach (string name in fullFolderName) {
                string[] split = name.Split("\\");
                ids.Add(split[split.Length-1]);
            }
            return ids;
        }

        public static void spawnFromData(SeralizedEntityData entityData, Transform container) {
            if (entityData == null) {
                return;
            }
            Vector2 position = new Vector2(entityData.x,entityData.y);
            switch (entityData.type) {
                case EntityType.Item:
                    spawnItemFromData(position,entityData.data,container);
                    break;
                case EntityType.Mob:
                    spawnMobFromData(position,entityData.data,container);
                    break;
                default:
                    Debug.LogWarning($"Tile Chunk Partition Loading does not cover case {entityData.type}");
                    break;
            }
        }

        private static void spawnItemFromData(Vector2 position, string data, Transform container) {
            ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlotFromString(data);
            ItemEntityFactory.SpawnItemEntity(position,itemSlot,container);
        }

        private static void spawnMobFromData(Vector2 position, string data, Transform container) {
            try {
                SerializedMobData serializedMobData = JsonConvert.DeserializeObject<SerializedMobData>(data);
                EntityRegistry.getInstance().spawnEntity(serializedMobData.id,position,serializedMobData.componentData,container);
            } catch (JsonSerializationException e) {
                Debug.LogError($"Error mob from data:\n{e}");
            }
        }
    }
}


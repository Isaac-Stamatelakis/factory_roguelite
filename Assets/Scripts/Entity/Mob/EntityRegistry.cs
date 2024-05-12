using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using Newtonsoft.Json;

namespace Entities.Mobs {
    public class EntityRegistry
    {
        private static readonly int MAX_CACHE_SIZE = 40;
        private Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

        private static EntityRegistry instance;
        private EntityRegistry() {
            cache = new Dictionary<string, GameObject>();
        }
        public static EntityRegistry getInstance() {
            if (instance == null) {
                instance = new EntityRegistry();
            }
            return instance;
        }

        public void spawnEntity(string id, Vector2 position, Dictionary<string,string> componentData, Transform container)
        {
            if (cache.ContainsKey(id)) {
                spawn(id,position,componentData, container);
            }
            else {
                loadEntityIntoMemory(id,position,componentData, container);
            }
        }

        private void spawn(string id, Vector2 position, Dictionary<string,string> componentData,Transform container) {
            GameObject entity = GameObject.Instantiate(cache[id]);
            entity.transform.position = position;
            MobEntity mobEntity = entity.GetComponent<MobEntity>();
            if (mobEntity == null) {
                Debug.LogError("Tried to spawn mob with id '" + id + "' which didn't have MobEntity Component");
                GameObject.Destroy(entity);
                return;
            }
            mobEntity.initalize();
            mobEntity.deseralize(id, componentData);
            entity.transform.SetParent(container);
            Debug.Log($"Spawned entity {entity.name} at position {position}");
        }

        private async void loadEntityIntoMemory(string id, Vector2 position, Dictionary<string,string> componentData,Transform container)
        {
            if (cache.ContainsKey(id)) {
                spawn(id,position,componentData,container);
            }
            string path = EntityUtils.getObjectPath(id);
            if (!Directory.Exists(path)) {
                Debug.LogWarning($"No entity folder found with id '{id}'");
                return;
            }

            string[] files = Directory.GetFiles(path);
            List<string> prefabFiles = new List<string>();
            foreach (string file in files) {
                if (file.Contains(".prefab")) {
                    prefabFiles.Add(file);
                }
            }
            if (prefabFiles.Count == 0) {
                Debug.LogError("Tried to load entity with id '" + id + "' that did not have a prefab");
                return;
            }
            if (prefabFiles.Count > 2) {
                Debug.LogError("Entity folder with id '" + id + "' has multiple prefabs");
            }
            string prefabFile = prefabFiles[0];
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(prefabFile);
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                //Debug.Log($"{id} loaded into cache");
                GameObject prefab = handle.Result;
                addToCache(id,prefab);
                spawn(id,position,componentData,container);
            }
            else
            {
                Debug.LogError($"Failed to load entity with ID: {id}");
            }
        }

        private void addToCache(string id, GameObject prefab) {
            if (cache.Count > MAX_CACHE_SIZE) {
                // Remove least recently used prefab
            }
            cache[id] = prefab;
        }
    }
}


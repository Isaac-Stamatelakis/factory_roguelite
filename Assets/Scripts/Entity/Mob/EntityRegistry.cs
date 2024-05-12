using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using Newtonsoft.Json;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule;
using System.Threading.Tasks;

namespace Entities.Mobs {
    public class EntityRegistry
    {
        private Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
        private Dictionary<string, Task> cachingTasks = new Dictionary<string, Task>();

        private static EntityRegistry instance;
        private EntityRegistry() {
            cache = new Dictionary<string, GameObject>();
            cachingTasks = new Dictionary<string, Task>();
        }
        public static EntityRegistry getInstance() {
            if (instance == null) {
                instance = new EntityRegistry();
            }
            return instance;
        }

        public async void spawnEntity(string id, Vector2 position, Dictionary<string,string> componentData, Transform container)
        {
            if (cache.ContainsKey(id)) {
                spawn(id,position,componentData, container);
            } else if (cachingTasks.ContainsKey(id)) {
                await cachingTasks[id];
                spawn(id,position,componentData,container);
            }
            else {
                loadEntityIntoMemoryThenSpawn(id,position,componentData, container);
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
            //Debug.Log($"Spawned entity {entity.name} at position {position}");
        }

        public void reset() {
            foreach (GameObject prefab in cache.Values) {
                Addressables.Release(prefab);
            }
            cache = new Dictionary<string, GameObject>();
        }

        public async Task cacheFromSystem(ClosedChunkSystem system) {
            foreach (ILoadedChunk chunk in system.CachedChunk.Values) {
                HashSet<string> ids = chunk.getEntityIds();
                foreach (string id in ids) {
                    if (cache.ContainsKey(id)) {
                        continue;
                    }
                    await loadEntityIntoMemeory(id);
                }
            }
        }

        private async Task loadEntityIntoMemeory(string id) {
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
            cachingTasks[id] = handle.Task;
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                addToCache(id,prefab);
            }
            else
            {
                Debug.LogError($"Failed to load entity with ID: {id}");
            }
        }

        private async void loadEntityIntoMemoryThenSpawn(string id, Vector2 position, Dictionary<string,string> componentData,Transform container)
        {
            await loadEntityIntoMemeory(id);
            spawn(id,position,componentData,container);
        }

        private void addToCache(string id, GameObject prefab) {
            //Debug.Log($"{id} loaded into cache");
            if (cache.ContainsKey(id)) {
                Addressables.Release(prefab);
                return;
            }
            cache[id] = prefab;
        }
    }
}


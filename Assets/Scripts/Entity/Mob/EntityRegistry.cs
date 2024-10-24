using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using Newtonsoft.Json;
using Chunks.Systems;
using Chunks;
using System.Threading.Tasks;
using System.Linq;

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

        public Dictionary<string, Vector2Int> getAllSizes() {
            List<string> ids = EntityUtils.getAllIds();
            Dictionary<string, Task> loadTask = new Dictionary<string, Task>();
            foreach (string id in ids) {
                if (cache.ContainsKey(id)) {
                    continue;
                }
                loadTask[id] = loadEntityIntoMemeory(id);
            }
            Task.WaitAll(loadTask.Values.ToArray());
            Dictionary<string,Vector2Int> sizeDict = new Dictionary<string, Vector2Int>();
            foreach (KeyValuePair<string,GameObject> kvp in cache) {
                SpriteRenderer spriteRenderer = kvp.Value.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) {
                    continue;
                }
                Vector3 size = spriteRenderer.bounds.size;
                Vector2Int vector = new Vector2Int(Mathf.FloorToInt(size.x/32),Mathf.FloorToInt(size.y/32));
                sizeDict[kvp.Key] = vector;
            }
            reset();
            return sizeDict;
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
            Debug.Log($"{cache.Count} entities cached for system {system.name}");
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
            cachingTasks.Remove(id);
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


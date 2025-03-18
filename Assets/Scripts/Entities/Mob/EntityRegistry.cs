using System;
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
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace Entities.Mobs {
    public class SerializedMobEntityData
    {
        public string Id;
        public string Data;
    }
    public class EntityRegistry : MonoBehaviour
    {
        private static Dictionary<string, IResourceLocation> assetLocationMap;
        private Dictionary<string, GameObject> cache = new ();
        private Dictionary<string,  AsyncOperationHandle<GameObject>> loadingHandleMap = new ();

        private static EntityRegistry instance;

        public static EntityRegistry Instance => instance;
        public void Awake()
        {
            instance = this;
        }

        public List<string> GetAllMobIds()
        {
            return assetLocationMap.Keys.ToList();
        }


        public static IEnumerator Initialize()
        {
            if (assetLocationMap != null) yield break;
            assetLocationMap = new Dictionary<string, IResourceLocation>();
            
            var handle = Addressables.LoadResourceLocationsAsync("entity");
            yield return handle;
            var result = handle.Result;
            foreach (var resourceLocation in result)
            {
                string entityId = Path.GetFileName(resourceLocation.InternalId).Replace(".prefab","").ToLower().Replace(" ","_");
                if (assetLocationMap.ContainsKey(entityId))
                {
                    Debug.LogWarning($"Duplicate entities {entityId}");
                }
                assetLocationMap[entityId] = resourceLocation;
            }
            Debug.Log($"Loaded {assetLocationMap.Count} Entity Asset References");
            Addressables.Release(handle);
        }

        public void StartEntitySpawnCoroutine(SerializedMobEntityData entityData, Vector2 position, Transform container)
        {
            StartCoroutine(SpawnEntityCoroutine(entityData, position, container));
        }

        public IEnumerator SpawnEntityCoroutine(SerializedMobEntityData entityData, Vector2 position, Transform container)
        {
            string id = entityData.Id;
            if (id == null) yield break;
            if (cache.ContainsKey(id))
            {
                Spawn(entityData, position, container);
            } else if (loadingHandleMap.TryGetValue(id, out var handle))
            {
                yield return handle;
                Spawn(entityData,position,container);
            }
            else
            {
                yield return LoadEntityThenSpawn(entityData,position, container);
            }
        }

        private void Spawn(SerializedMobEntityData serializedEntityData, Vector2 position,Transform container) {
            GameObject entity = Object.Instantiate(cache[serializedEntityData.Id], container, false);
            MobEntity mobEntity = entity.GetComponent<MobEntity>();
            if (!mobEntity) {
                Debug.LogError("Tried to spawn mob with id '" + serializedEntityData.Id + "' which didn't have MobEntity Component");
                Object.Destroy(entity);
                return;
            }
            entity.transform.position = position;
            mobEntity.initalize();
            mobEntity.Deseralize(serializedEntityData.Id, serializedEntityData.Data);
            
        }
        

        public void Reset() {
            cache.Clear();
        }

        private IEnumerator LoadEntityIntoMemory(string id)
        {
            if (!assetLocationMap.TryGetValue(id, out IResourceLocation location)) yield break;
            var handle = Addressables.LoadAssetAsync<GameObject>(location);
            loadingHandleMap[id] = handle;
            yield return handle;
            loadingHandleMap.Remove(id);
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
        
        private IEnumerator LoadEntityThenSpawn(SerializedMobEntityData serializedEntityData, Vector2 position, Transform container)
        {
            yield return LoadEntityIntoMemory(serializedEntityData.Id);
            Spawn(serializedEntityData,position,container);
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


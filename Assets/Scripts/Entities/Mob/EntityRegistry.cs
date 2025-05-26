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
        public float Health;
        public float Size;
        public Dictionary<SerializableMobComponentType,string> ComponentDataDict;

        public SerializedMobEntityData(string id, float health, float size, Dictionary<SerializableMobComponentType,string> componentDataDict)
        {
            Id = id;
            Health = health;
            Size = size;
            ComponentDataDict = componentDataDict;
        }
    }
    public class EntityRegistry : MonoBehaviour
    {
        private static Dictionary<string, IResourceLocation> assetLocationMap;
        private Dictionary<string, MobEntity> cache = new ();
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
                
                if (handle.IsValid()) // Work around, this is slightly ineffiecent
                {
                    GameObject prefab = handle.Result;
                    cache[id] = prefab.GetComponent<MobEntity>();
                }
                
                Spawn(entityData,position,container);
            }
            else
            {
                yield return LoadEntityThenSpawn(entityData,position, container);
            }
        }

        public MobEntity GetEntityPrefab(string entityId)
        {
            return cache.GetValueOrDefault(entityId);
        }

        private void Spawn(SerializedMobEntityData serializedEntityData, Vector2 position,Transform container) {
            MobEntity entity = Object.Instantiate(cache[serializedEntityData.Id], container, true);
            MobEntity mobEntity = entity.GetComponent<MobEntity>();
            if (!mobEntity) {
                Debug.LogError("Tried to spawn mob with id '" + serializedEntityData.Id + "' which didn't have MobEntity Component");
                Object.Destroy(entity);
                return;
            }
            entity.transform.localPosition = position;
            mobEntity.Initialize();
            mobEntity.Deserialize(serializedEntityData);
        }
        

        public void ClearCache() {
            cache.Clear();
        }

        public void ClearCache(List<string> idsToPreserve)
        {
            List<string> loadedIds = cache.Keys.ToList();
            foreach (string id in loadedIds)
            {
                if (idsToPreserve.Contains(id)) continue;
                cache.Remove(id);
            }
        }

        public IEnumerator LoadEntitiesIntoMemory(List<string> ids)
        {
            List<string> idsBeingLoaded = new();
            List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();
            foreach (var id in ids)
            {
                if (!assetLocationMap.TryGetValue(id, out IResourceLocation location)) continue;
                if (cache.ContainsKey(id)) continue;
                var handle = StartEntityLoad(id, location);
                handles.Add(handle);
                idsBeingLoaded.Add(id);
            }

            for (var index = 0; index < handles.Count; index++)
            {
                var handle = handles[index];
                yield return handle;
                OnHandleComplete(idsBeingLoaded[index], handle);
            }
            Debug.Log($"Loaded {idsBeingLoaded.Count} Entities into memory");
        }

        private IEnumerator LoadEntityIntoMemory(string id)
        {
            if (!assetLocationMap.TryGetValue(id, out IResourceLocation location)) yield break;
            var handle = StartEntityLoad(id, location);
            yield return handle;
            OnHandleComplete(id, handle);
        }

        private AsyncOperationHandle<GameObject> StartEntityLoad(string id, IResourceLocation location)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(location);
            loadingHandleMap[id] = handle;
            return handle;
        }

        private void OnHandleComplete(string id, AsyncOperationHandle<GameObject> handle)
        {
            loadingHandleMap.Remove(id);
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject prefab = handle.Result;
                cache[id] = prefab.GetComponent<MobEntity>();
            }
            else
            {
                Debug.LogError($"Failed to load entity with ID: {id}");
            }
            Addressables.Release(handle);
        }
        
        private IEnumerator LoadEntityThenSpawn(SerializedMobEntityData serializedEntityData, Vector2 position, Transform container)
        {
            if (!assetLocationMap.ContainsKey(serializedEntityData.Id)) yield break;
            yield return LoadEntityIntoMemory(serializedEntityData.Id);
            Spawn(serializedEntityData,position,container);
        }
    }
}


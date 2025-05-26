using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;
using Entities.Mobs;
using Entities;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;
using World.Cave.Registry;
using Random = UnityEngine.Random;

namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Entity Distributor",menuName="Generation/Entity Distributor")]
    public class CaveEntityDistrubtionObject : ScriptableObject
    {
        public List<EntityDistribution> entities;
    }

    public class CaveEntityDistributor : ICaveDistributor
    {
        private List<EntityDistribution> entities;
        private CaveElements caveElements;
        private CaveObject caveObject;

        public CaveEntityDistributor(List<EntityDistribution> distributions, CaveObject caveObject, CaveElements caveElements)
        {
            this.entities = distributions;
            this.caveElements = caveElements;
            this.caveObject = caveObject;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            EntityRegistry entityRegistry = EntityRegistry.Instance;
            CaveTileCollection caveTileCollection = CaveRegistry.Instance.GetCaveTileCollection(caveObject.GetId());
            foreach (EntityDistribution entityDistribution in entities)
            {
                MobEntity mobEntityPrefab = entityRegistry.GetEntityPrefab(entityDistribution.entityId);
                bool error = false;
                if (!mobEntityPrefab)
                {
                    Debug.LogWarning("Entity " + entityDistribution.entityId + " is null");
                    error = true;
                }
                
                if (error)
                {
                    continue;
                }
                SpriteRenderer spriteRenderer = mobEntityPrefab.GetComponent<SpriteRenderer>();
                Vector2Int spriteSize = spriteRenderer
                    ? Global.GetSpriteSize(spriteRenderer.sprite)
                    : Vector2Int.one;
                
                int amount = StatUtils.getAmount(entityDistribution.mean,entityDistribution.standardDeviation);
                MobSpawnCondition spawnCondition = mobEntityPrefab.MobSpawnCondition;
                while (amount > 0) {
                    int spawnAttempts = 64;
                    while (spawnAttempts > 0) {
                        int ranX = Random.Range(0,width-spriteSize.x);
                        int ranY = Random.Range(0,height-spriteSize.y);
                        if (!CanPlaceEntity(worldData, spawnCondition, new Vector2Int(ranX, ranY), spriteSize))
                        {
                            spawnAttempts--;
                            continue;
                        }

                        Dictionary<SerializableMobComponentType, string> componentDataDict;
                        ISerializableMobComponent[] components = mobEntityPrefab.GetComponents<ISerializableMobComponent>();
                        
                        if (components.Length == 0)
                        {
                            componentDataDict = null;
                        }
                        else
                        {
                            componentDataDict = GetInitializeCaveData(components);
                        }

                        float randomSize = mobEntityPrefab.RandomizeSize ? MobEntity.GetRandomSize() : 1f;
                        SerializedMobEntityData mobEntityData = new SerializedMobEntityData(entityDistribution.entityId, mobEntityPrefab.Health, randomSize,componentDataDict);
                        string mobData = JsonConvert.SerializeObject(mobEntityData);
                        Vector2 spawnPosition = ((new Vector2(ranX,ranY+1))+bottomLeftCorner)/2f;
                        worldData.entityData.Add(
                            new SeralizedEntityData(
                                type: EntityType.Mob,
                                position: spawnPosition,
                                data: mobData
                            )
                        );
                        break;
                    }
                    amount--;
                }
                
            }
            Debug.Log($"Spawned {worldData.entityData.Count} Entities inside Cave");

            return;
            Dictionary<SerializableMobComponentType, string> GetInitializeCaveData(ISerializableMobComponent[] components)
            {
                var componentDataDict = new Dictionary<SerializableMobComponentType, string>();
                foreach (ISerializableMobComponent component in components)
                {
                    if (component is ICaveInitiazableMobComponent initializableSerializableMobComponent)
                    {
                        initializableSerializableMobComponent.Initialize(componentDataDict, caveTileCollection); 
                    }
                }
                return componentDataDict;
            }
        }
        

        private bool CanPlaceEntity(SeralizedWorldData worldData, MobSpawnCondition mobSpawnCondition, Vector2Int tilePosition, Vector2Int spriteSize)
        {
            if (mobSpawnCondition == MobSpawnCondition.None) return true;
            for (int x = 0; x < spriteSize.x; x++)
            {
                for (int y = 0; y < spriteSize.y; y++)
                {
                    Vector2Int position = new Vector2Int(tilePosition.x + x, tilePosition.y + y);
                    if (worldData.baseData.ids[position.x, position.y] != null) return false;
                }
            }

            bool underWater = IsUnderWater(worldData, tilePosition, spriteSize);
            
            if (underWater)
            {
                return mobSpawnCondition == MobSpawnCondition.InWater;
            }
            
            if (mobSpawnCondition == MobSpawnCondition.InWater) return false;
                
            if (mobSpawnCondition == MobSpawnCondition.InAir) return true;
            return IsOnGround(worldData,tilePosition,spriteSize);

        }

        private bool IsUnderWater(SeralizedWorldData worldData, Vector2Int tilePosition, Vector2Int spriteSize)
        {
            for (int x = 0; x < spriteSize.x; x++)
            {
                for (int y = 0; y < spriteSize.y; y++)
                {
                    Vector2Int position = new Vector2Int(tilePosition.x + x, tilePosition.y + y);
                    const float MAX_FILL = 1f;
                    if (!Mathf.Approximately(worldData.fluidData.fill[position.x, position.y], MAX_FILL)) return false;
                }
            }
            return true;
        }

        private bool IsOnGround(SeralizedWorldData worldData, Vector2Int tilePosition, Vector2Int spriteSize)
        {
            if (tilePosition.y < 1) return true;
            for (int x = 0; x < spriteSize.x; x++)
            {
                Vector2Int position = new Vector2Int(tilePosition.x + x, tilePosition.y-1);
                if (worldData.baseData.ids[position.x, position.y] == null) return false;
            }

            return true;
        }
    }
}


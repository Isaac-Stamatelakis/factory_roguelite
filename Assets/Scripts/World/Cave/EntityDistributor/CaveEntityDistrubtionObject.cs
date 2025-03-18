using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;
using Entities.Mobs;
using Entities;
using UnityEngine.AddressableAssets;
using Newtonsoft.Json;

namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Entity Distributor",menuName="Generation/Entity Distributor")]
    public class CaveEntityDistrubtionObject : ScriptableObject
    {
        public List<EntityDistribution> entities;
    }

    public class CaveEntityDistributor : ICaveDistributor
    {
        private List<EntityDistribution> entities;

        public CaveEntityDistributor(List<EntityDistribution> distributions)
        {
            this.entities = distributions;
        }

        public void Distribute(SeralizedWorldData worldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            EntityRegistry entityRegistry = EntityRegistry.Instance;
            foreach (EntityDistribution entityDistribution in entities)
            {
                Vector2Int? conditionalSize = entityRegistry.GetSizeOfEntity(entityDistribution.entityId);
                if (!conditionalSize.HasValue)
                {
                    Debug.LogWarning("Entity " + entityDistribution.entityId + " has size data");
                    continue;
                }
                Vector2Int entitySize = conditionalSize.Value;
                int amount = StatUtils.getAmount(entityDistribution.mean,entityDistribution.standardDeviation);
                
                while (amount > 0) {
                    int spawnAttempts = 25;
                    while (spawnAttempts > 0) {
                        int ranX = Random.Range(0,width-entitySize.x);
                        int ranY = Random.Range(0,height-entitySize.y);
                        for (int x = 0; x < entitySize.x; x++) {
                            for (int y = 0; y < entitySize.y; y++)
                            {
                                if (worldData.baseData.ids[x + ranX, y + ranY] == null) continue;
                                
                                spawnAttempts--;
                            }
                        }
                        string mobData = JsonConvert.SerializeObject(new SerializedMobData(entityDistribution.entityId,null));
                        Vector2 spawnPosition = ((new Vector2(ranX,ranY))+bottomLeftCorner)/2f;
                        worldData.entityData.Add(
                            new SeralizedEntityData(
                                type: EntityType.Mob,
                                position: spawnPosition,
                                data: mobData
                            )
                        );
                    }
                    amount--;
                }
                
            }
        }
    }
}


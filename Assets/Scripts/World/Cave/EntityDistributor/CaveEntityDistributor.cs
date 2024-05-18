using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Misc;
using Entities.Mobs;
using Entities;
using Newtonsoft.Json;

namespace WorldModule.Caves {
    [CreateAssetMenu(fileName ="New Entity Distributor",menuName="Generation/Entity Distributor")]
    public class CaveEntityDistributor : ScriptableObject, ICaveDistributor
    {
        public List<EntityDistribution> entities;
        public void distribute(SeralizedWorldData seralizedWorldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            Dictionary<string,Vector2Int> sizeDict = EntityRegistry.getInstance().getAllSizes();
            foreach (EntityDistribution entityDistribution in entities) {
                int amount = StatUtils.getAmount(entityDistribution.mean,entityDistribution.standardDeviation);
                Debug.Log(amount);
                if (!sizeDict.ContainsKey(entityDistribution.entityId)) {
                    Debug.LogWarning($"Id {entityDistribution.entityId} was not found");
                    continue;
                }
                while (amount > 0) {
                    int spawnAttempts = 25;
                    Vector2Int size = sizeDict[entityDistribution.entityId];
                    while (spawnAttempts > 0) {
                        int ranX = Random.Range(0,width-size.x);
                        int ranY = Random.Range(0,height-size.y);
                        for (int x = 0; x < size.x; x++) {
                            for (int y = 0; y < size.y; y++) {
                                if (seralizedWorldData.baseData.ids[x+ranX,y+ranY] != null) {
                                    spawnAttempts--;
                                    continue;
                                }
                            }
                        }
                        string mobData = JsonConvert.SerializeObject(new SerializedMobData(entityDistribution.entityId,null));
                        seralizedWorldData.entityData.Add(
                            new SeralizedEntityData(
                                type: EntityType.Mob,
                                position: new Vector2(ranX/2f,ranY/2f),
                                data: mobData
                            )
                        );
                        Debug.Log($"Spawned at {ranX}, {ranY}");
                        break;
                    }
                    amount--;
                }
                
            }
        }
    }
}


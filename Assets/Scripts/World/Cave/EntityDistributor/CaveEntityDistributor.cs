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
    public class CaveEntityDistributor : ScriptableObject, ICaveDistributor
    {
        public List<EntityDistribution> entities;
        public void distribute(SeralizedWorldData seralizedWorldData, int width, int height, Vector2Int bottomLeftCorner)
        {
            //Dictionary<string,Vector2Int> sizeDict = EntityRegistry.getInstance().getAllSizes();
            Dictionary<string,Vector2Int> sizeDict = new Dictionary<string, Vector2Int>();
            foreach (EntityDistribution entityDistribution in entities) {
                int amount = StatUtils.getAmount(entityDistribution.mean,entityDistribution.standardDeviation);
                if (!sizeDict.ContainsKey(entityDistribution.entityId)) {
                    Debug.LogWarning($"Id {entityDistribution.entityId} was not found");
                    continue;
                }
                while (amount > 0) {
                    int spawnAttempts = 25;
                    Vector2Int entitySize = sizeDict[entityDistribution.entityId];
                    while (spawnAttempts > 0) {
                        int ranX = Random.Range(0,width-entitySize.x);
                        int ranY = Random.Range(0,height-entitySize.y);
                        for (int x = 0; x < entitySize.x; x++) {
                            for (int y = 0; y < entitySize.y; y++) {
                                if (seralizedWorldData.baseData.ids[x+ranX,y+ranY] != null) {
                                    spawnAttempts--;
                                    continue;
                                }
                            }
                        }
                        string mobData = JsonConvert.SerializeObject(new SerializedMobData(entityDistribution.entityId,null));
                        Vector2 spawnPosition = ((new Vector2(ranX,ranY))+bottomLeftCorner)/2f;
                        seralizedWorldData.entityData.Add(
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
        }
    }
}


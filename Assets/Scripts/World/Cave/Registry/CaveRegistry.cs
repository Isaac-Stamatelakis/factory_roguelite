using System.Collections;
using System.Collections.Generic;
using Items;
using Items.Transmutable;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WorldModule.Caves;

namespace World.Cave.Registry
{
    public class CaveRegistry : MonoBehaviour
    {
        private static CaveRegistry instance;
        private Dictionary<string, CaveTileCollection> caveDataDict;

        public void Awake()
        {
            caveDataDict = new Dictionary<string, CaveTileCollection>();
            instance = this;
            StartCoroutine(Initialize());
        }

        private IEnumerator Initialize()
        {
            var handle = Addressables.LoadAssetsAsync<CaveObject>("cave", null);
            yield return handle;
            
            
            var caves = handle.Result;
            List<AsyncOperationHandle<GenerationModel>> genModelHandles = new List<AsyncOperationHandle<GenerationModel>>();
            foreach (CaveObject caveObject in caves)
            {
                var genModelHandle = Addressables.LoadAssetAsync<GenerationModel>(caveObject.generationModel);
                genModelHandles.Add(genModelHandle);
            }

            for (var index = 0; index < genModelHandles.Count; index++)
            {
                var genModelHandle = genModelHandles[index];
                yield return genModelHandle;
                var genModel = genModelHandle.Result;
                Addressables.Release(genModelHandle);
                
                CaveObject caveObject = caves[index];
                string id = caveObject.name.ToLower().Replace(" ", "_");
                CaveTileCollection caveTileCollection = CreateCaveTileCollection(caveObject,genModel.GetBaseId());
                if (caveTileCollection == null) continue;
                caveDataDict[id] = caveTileCollection;
                
            }

            Addressables.Release(handle);
            foreach (var (id, caveCollection) in caveDataDict)
            {
                Debug.Log($"{id} {caveCollection}");
            }
            
        }

        private CaveTileCollection CreateCaveTileCollection(CaveObject caveObject, string baseId)
        {
            ItemRegistry itemRegistry = ItemRegistry.GetInstance();
            List<float> odds = new List<float>();
            List<string> ids = new List<string>();
            List<string> tileIds = new List<string>();
            if (caveObject.TileDistributorObject)
            {
                foreach (var tileDistribution in caveObject.TileDistributorObject.TileDistributions)
                {
                    float fill = tileDistribution.TileDistributionData.Fill;
                    if (tileDistribution.TileDistributionData.WriteAll)
                    {
                        float reduction = 1 - fill;
                        for (int i = 0; i < odds.Count; i++)
                        {
                            odds[i] *= reduction;
                        }
                    }

                    uint cumulativeFrequency = 0;
                    foreach (var tileFrequency in tileDistribution.Tiles)
                    {
                        cumulativeFrequency += tileFrequency.frequency;
                    }
                    if (cumulativeFrequency == 0) continue;

                    foreach (var tileFrequency in tileDistribution.Tiles)
                    {
                        float chance = (tileFrequency.frequency * fill) / cumulativeFrequency;
                        odds.Add(chance);
                        ids.Add(tileFrequency.tileItem?.id);
                        tileIds.Add(tileFrequency.tileItem?.id);
                    }
                }
            }

            if (caveObject.OreDistributionObject)
            {
                foreach (var oreDistribution in caveObject.OreDistributionObject.OreDistributions)
                {
                    float fill = oreDistribution.TileDistributionData.Fill;
                    string oreItemId = TransmutableItemUtils.GetStateId(oreDistribution.Material,TransmutableItemState.Ore);
                    odds.Add(fill);
                    ids.Add(oreItemId);
                    for (var index = 0; index < tileIds.Count; index++)
                    {
                        var tileId = tileIds[index];
                        string tileOreId = TransmutableItemUtils.GetOreId(tileId, oreDistribution.Material);
                        TileItem tileItem = itemRegistry.GetTileItem(tileOreId);
                        if (!tileItem) continue;
                        odds[index] *= 1 - fill;
                    }
                }
            }
            
            if (odds.Count == 0) return null;
            float[] cumulativeOdds = new float[odds.Count];
            cumulativeOdds[0] = odds[0];
            for (int i = 1; i < odds.Count; i++)
            {
                cumulativeOdds[i] = cumulativeOdds[i - 1] + odds[i];
            }
            return new CaveTileCollection(cumulativeOdds, ids.ToArray(), baseId);
        }
    }

    
    public class CaveTileCollection
    {
        public CaveTileCollection(float[] cumulativeOdds, string[] ids, string baseId)
        {
            this.cumulativeOdds = cumulativeOdds;
            this.ids = ids;
            this.baseId = baseId;
        }

        private float[] cumulativeOdds;
        private string[] ids;
        private string baseId;
        
        public string GetId(float value)
        {
            const int NOT_FOUND = -1;
            int index = BinarySearch(cumulativeOdds, value);
            return index == NOT_FOUND ? baseId : ids[index];
        }
        
        private static int BinarySearch(float[] cumulativeProbabilities, float value)
        {
            int left = 0;
            int right = cumulativeProbabilities.Length - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (cumulativeProbabilities[mid] < value)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }
            return left;
        }

        public override string ToString()
        {
            string result = $"CaveTileCollection: BaseID {baseId}\n";
            for (var index = 0; index < cumulativeOdds.Length; index++)
            {
                float cumulativeOdd = cumulativeOdds[index];
                string id = ids[index];
                result += $"{id}:{cumulativeOdd:F2} ";
            }
            return result;
        }
    }
}

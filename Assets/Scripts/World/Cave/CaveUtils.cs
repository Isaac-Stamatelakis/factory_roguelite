using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Dimensions;
using Entities.Mobs;
using Player;
using UI.Chat;
using UI.Statistics;
using Unity.VisualScripting;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using World.Cave.TileDistributor.Ore;
using World.Cave.TileDistributor.Standard;
using Debug = UnityEngine.Debug;

namespace WorldModule.Caves {
    
    public static class CaveUtils {
        public static IEnumerable LoadCaveElements(CaveObject caveObject) {
            var entityHandle = caveObject.entityDistributor.LoadAssetAsync<UnityEngine.Object>();
            var generationModelHandle = caveObject.generationModel.LoadAssetAsync<UnityEngine.Object>();
            
            yield return entityHandle;
            yield return generationModelHandle;
            
        }

        public static string IdFromName(string name)
        {
            return name.ToLower().Replace(" ", "_");
        }

        public static string NameFromId(string id)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id.Replace("_"," "));
        }
        
        public static IEnumerator LoadCave(CaveObject caveObject, CaveCallback caveCallback) {
            CaveElements caveElements = new CaveElements();
            Dictionary<string, AsyncOperationHandle<Object>> handles = new Dictionary<string, AsyncOperationHandle<Object>>();
            if (caveObject.generationModel == null) {
                Debug.LogError($"Cannot teleport to cave {caveObject.name}: does not have a generation model");
                yield break;
            }

            handles["Model"] = caveObject.generationModel.LoadAssetAsync<Object>();
            if (caveObject.entityDistributor.RuntimeKeyIsValid()) {
                handles["Entity"] = Addressables.LoadAssetAsync<Object>(caveObject.entityDistributor);
            }

            if (caveObject.structureDistributor.RuntimeKeyIsValid())
            {
                handles["Structure"] = Addressables.LoadAssetAsync<Object>(caveObject.structureDistributor);
            }
            

            List<AsyncOperationHandle<Object>> songHandles = new List<AsyncOperationHandle<Object>>();
            foreach (AssetReference assetReference in caveObject.songs) {
                if (assetReference.RuntimeKeyIsValid()) {
                    songHandles.Add(assetReference.LoadAssetAsync<Object>());
                }
                
            }

            foreach (var kvp in handles) {
                yield return kvp.Value;
            }
            
            foreach (var handle in songHandles) {
                yield return handle;
            }
            
            EntityRegistry entityRegistry = EntityRegistry.Instance;
            if (handles.TryGetValue("Entity", out var entityHandle) && entityHandle.Result is CaveEntityDistrubtionObject caveEntityDistrubtionObject)
            {
                List<EntityDistribution> entityDistributions = caveEntityDistrubtionObject.entities;
                List<string> ids = new List<string>();
                foreach (EntityDistribution entityDistribution in entityDistributions)
                {
                    ids.Add(entityDistribution.entityId);
                }
                entityRegistry.ClearCache(ids);
                yield return entityRegistry.LoadEntitiesIntoMemory(ids);
                caveElements.CaveEntityDistributor = new CaveEntityDistributor(entityDistributions);;
            } else
            {
                entityRegistry.ClearCache();
            }
            if (handles.TryGetValue("Structure", out var structureHandle)) {
                caveElements.StructureDistributor = AddressableUtils.validateHandle<AreaStructureDistributor>(structureHandle);
            }
            
            caveElements.GenerationModel = AddressableUtils.validateHandle<GenerationModel>(handles["Model"]);
            caveElements.Songs = AddressableUtils.validateHandles<AudioClip>(songHandles);

            if (caveObject.TileDistributorObject)
            {
                List<TileDistribution> tileDistributions = new List<TileDistribution>();
                foreach (StandardTileDistrubtion distributorObjectData in caveObject.TileDistributorObject.TileDistributions)
                {
                    if (distributorObjectData == null) continue;
                    List<TileDistributionFrequency> tileDistributionFrequencies = new List<TileDistributionFrequency>();
                    foreach (TileDistributionFrequency frequency in distributorObjectData.Tiles)
                    {
                        if (frequency.frequency == 0 || frequency.tileItem?.id == null) continue;
                        tileDistributionFrequencies.Add(frequency);
                    }
                    FrequencyTileAggregator frequencyTileAggregator = new FrequencyTileAggregator(tileDistributionFrequencies);
                    tileDistributions.Add(new TileDistribution(frequencyTileAggregator, distributorObjectData.TileDistributionData));
                }
            
                caveElements.TileDistributor = new AreaTileDistributor(tileDistributions,caveElements.GenerationModel.GetBaseId());
            }

            if (caveObject.OreDistributionObject)
            {
                List<TileDistribution> tileDistributions = new List<TileDistribution>();
                foreach (OreDistribution oreDistribution in caveObject.OreDistributionObject.OreDistributions)
                {
                    OreTileAggregator oreTileAggregator = new OreTileAggregator(oreDistribution.Material);
                    tileDistributions.Add(new TileDistribution(oreTileAggregator,oreDistribution.TileDistributionData));

                }
                caveElements.OreDistributor = new AreaTileDistributor(tileDistributions,caveElements.GenerationModel.GetBaseId());
            }
            
            foreach (var kvp in handles) {
                Addressables.Release(kvp.Value);
            }
            
            foreach (var handle in songHandles) {
                Addressables.Release(handle);
            }
            CaveInstance caveInstance = new CaveInstance(caveObject,caveElements);
            
            yield return caveCallback(caveInstance);

        }
        
        public delegate IEnumerator CaveCallback(CaveInstance caveInstance);

        public static IEnumerator GenerateAndTeleportToCave(CaveInstance caveInstance) {
            if (WorldLoadUtils.DimExists(-1)) {
                string path = WorldLoadUtils.GetDimPath(-1);
                Directory.Delete(path, true);
            }
            WorldLoadUtils.createDimFolder(-1);
            
            IntervalVector coveredArea = caveInstance.CaveObject.GetChunkCoveredArea();
            SeralizedWorldData worldTileData = caveInstance.Generate(UnityEngine.Random.Range(int.MinValue,int.MaxValue));
            Vector2Int bottomLeftCorner = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.CHUNK_SIZE;
            CaveSpawnPositionSearcher caveSpawnPositionSearcher = new CaveSpawnPositionSearcher(worldTileData,bottomLeftCorner,Vector2Int.zero,65536);
            Vector2Int spawnPosition = caveSpawnPositionSearcher.search();
            worldTileData.baseData.ids[spawnPosition.x-bottomLeftCorner.x, spawnPosition.y-bottomLeftCorner.y] = "return_portal";
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            yield return WorldGenerationFactory.SaveToJsonCoroutine(
                worldTileData,
                caveInstance.CaveObject.GetChunkCaveSize(),
                -1,
                WorldLoadUtils.GetDimPath(-1)
            );
            
            Debug.Log($"Serialized cave data in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            stopwatch.Stop();
            
            Vector2 worldSpawnPosition = spawnPosition * Vector2.one * Global.TILE_SIZE;
            
            Debug.Log("Teleporting to " + caveInstance.CaveObject.name);
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            playerScript.PlayerStatisticCollection.DiscreteValues[PlayerStatistic.Caves_Explored]++;
            DimensionManager dimensionManager = DimensionManager.Instance;
            CaveController caveController = (CaveController)dimensionManager.GetDimController(Dimension.Cave);
            caveController.setCurrentCave(caveInstance.CaveObject,worldSpawnPosition);

            CaveOptions caveOptions = caveInstance.CaveObject.CaveOptions;

            DimensionOptions dimensionOptions = new DimensionOptions(caveOptions);
            
            DimensionManager.Instance.SetPlayerSystem(playerScript, Dimension.Cave,worldSpawnPosition,dimensionOptions: dimensionOptions);
            
            TextChatUI.Instance.SendChatMessage($"Teleported to <b><color=purple>{caveInstance.CaveObject.name}!</color></b>\nPress <b>[KEY]</b> to return to the hub!");
        }
    }
}


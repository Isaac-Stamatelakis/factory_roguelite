using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Caves;
using Dimensions;
using PlayerModule;
using WorldModule;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

namespace TileEntity.Instances {
    public class CaveSelectController : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button teleportButton;
        private Cave currentCave;
        private CaveInstance caveInstance;
        public void display(CaveTeleporterInstance tileEntityInstance)
        {
            
        }

        public void showCave(Cave cave) {
            teleportButton.onClick.RemoveAllListeners();
            teleportButton.onClick.AddListener(() => {
                StartCoroutine(teleportButtonPress());
            });
            teleportButton.gameObject.SetActive(true);
            currentCave = cave;
            nameText.text = cave.name;
            descriptionText.text = cave.Description;
        }

        public void showDefault() {
            nameText.text = "";
            descriptionText.text = "Click on a cave to view";
            teleportButton.gameObject.SetActive(false);
        }

        private IEnumerator teleportButtonPress() {
            if (currentCave == null) {
                Debug.LogError("Tried to teleport to null cave");
                yield break;
            }
            yield return StartCoroutine(loadCave(currentCave,generateAndTeleportToCave));
        }

        public IEnumerator loadCave(Cave cave, CaveCallback caveCallback) {
            CaveElements caveElements = new CaveElements();
            Dictionary<string, AsyncOperationHandle<Object>> handles = new Dictionary<string, AsyncOperationHandle<Object>>();
            if (cave.generationModel == null) {
                Debug.LogError($"Cannot teleport to cave {cave.name}: does not have a generation model");
                yield break;
            }

            handles["Model"] = cave.generationModel.LoadAssetAsync<Object>();
            if (cave.entityDistributor.RuntimeKeyIsValid()) {
                handles["Entity"] = Addressables.LoadAssetAsync<Object>(cave.entityDistributor);
            }
            

            List<AsyncOperationHandle<Object>> tileDistributorHandles = new List<AsyncOperationHandle<Object>>();
            foreach (AssetReference assetReference in cave.tileGenerators) {
                if (assetReference.RuntimeKeyIsValid()) {
                    tileDistributorHandles.Add(assetReference.LoadAssetAsync<Object>());
                }
                
            }

            List<AsyncOperationHandle<Object>> songHandles = new List<AsyncOperationHandle<Object>>();
            foreach (AssetReference assetReference in cave.songs) {
                if (assetReference.RuntimeKeyIsValid()) {
                    songHandles.Add(assetReference.LoadAssetAsync<Object>());
                }
                
            }

            foreach (var kvp in handles) {
                yield return kvp.Value;
            }
            
            foreach (var handle in tileDistributorHandles) {
                yield return handle;
            }
            foreach (var handle in songHandles) {
                yield return handle;
            }
            if (handles.ContainsKey("Entity")) {
                caveElements.EntityDistributor = AddressableUtils.validateHandle<CaveEntityDistributor>(handles["Entity"]);
            }
            
            caveElements.GenerationModel = AddressableUtils.validateHandle<GenerationModel>(handles["Model"]);
            caveElements.Songs = AddressableUtils.validateHandles<AudioClip>(songHandles);
            caveElements.TileGenerators = AddressableUtils.validateHandles<CaveTileGenerator>(tileDistributorHandles);

            CaveInstance caveInstance = new CaveInstance(cave,caveElements);
            caveCallback(caveInstance);
            foreach (var kvp in handles) {
                Addressables.Release(kvp.Value);
            }
            foreach (var handle in tileDistributorHandles) {
                Addressables.Release(handle);
            }
            foreach (var handle in songHandles) {
                Addressables.Release(handle);
            }

        }
        public delegate void CaveCallback(CaveInstance caveInstance);

        public void generateAndTeleportToCave(CaveInstance caveInstance) {
            if (WorldLoadUtils.dimExists(-1)) {
                string path = WorldLoadUtils.getDimPath(-1);
                Directory.Delete(path, true);
            }
            WorldLoadUtils.createDimFolder(-1);
            SeralizedWorldData worldTileData = caveInstance.generate(UnityEngine.Random.Range(-2147483648,2147483647));
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            WorldGenerationFactory.SaveToJson(
                worldTileData,
                caveInstance.getChunkCaveSize(),
                -1,
                WorldLoadUtils.getDimPath(-1)
            );
            Debug.Log($"Serialized cave data in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
            stopwatch.Stop();
            IntervalVector coveredArea = caveInstance.getChunkCoveredArea();
            Vector2Int bottomLeftCorner = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.ChunkSize;
            CaveSpawnPositionSearcher caveSpawnPositionSearcher = new CaveSpawnPositionSearcher(worldTileData,bottomLeftCorner,Vector2Int.zero,65536);
            Vector2Int spawnPosition = caveSpawnPositionSearcher.search();
            Debug.Log("Teleporting to " + currentCave.name);
            Transform playerTransform = PlayerContainer.getInstance().getTransform();
            DimensionManager dimensionManager = DimensionManager.Instance;
            CaveController caveController = (CaveController)dimensionManager.getDimController(-1);
            caveController.setCurrentCave(caveInstance);
            
            DimensionManager.Instance.setPlayerSystem(playerTransform, -1,spawnPosition);
        }
    }
}


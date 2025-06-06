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
using Tiles.TileMap.Interval;
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
        public static string IdFromName(string name)
        {
            return name.ToLower().Replace(" ", "_");
        }

        public static string NameFromId(string id)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(id.Replace("_"," "));
        }
        
        public static IEnumerator LoadCave(CaveObject caveObject, CaveCallback caveCallback) {
            CaveInstance caveInstance = new CaveInstance(caveObject);
            
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
            IEnumerator worldTileDataEnumerator = caveInstance.Generate(UnityEngine.Random.Range(int.MinValue,int.MaxValue));
            yield return worldTileDataEnumerator;
            SeralizedWorldData worldTileData = worldTileDataEnumerator.Current as SeralizedWorldData;
            
            Vector2Int bottomLeftCorner = new Vector2Int(coveredArea.X.LowerBound,coveredArea.Y.LowerBound)*Global.CHUNK_SIZE;
            CaveSpawnPositionSearcher caveSpawnPositionSearcher = new CaveSpawnPositionSearcher(worldTileData,bottomLeftCorner,Vector2Int.zero,65536);
            Vector2Int spawnPosition = caveSpawnPositionSearcher.Search();
            
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.Systems;
using Entities.Mobs;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using Tiles;
using Items;
using Player;
using Player.Tool;
using RecipeModule;
using PlayerModule;
using Recipe;
using UI;
using UI.JEI;
using UI.QuestBook;
using UnityEngine.Rendering;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Dimensions {
    public interface ICompactMachineDimManager {
        public CompactMachineDimController GetCompactMachineDimController();
    }
    public abstract class DimensionManager : MonoBehaviour
    {
        [SerializeField] private DimensionObjects miscObjects;
        private static DimensionManager instance;
        public static DimensionManager Instance {get => instance;}
        public void Awake() {
            instance = this;
        }
        protected DimController currentDimension;
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        private Dictionary<Transform, PlayerWorldData> playerWorldData = new Dictionary<Transform, PlayerWorldData>(); 
        private Dictionary<ClosedChunkSystem, Vector2Int> activeSystems = new Dictionary<ClosedChunkSystem, Vector2Int>();

        public void Start() {
            StartCoroutine(initalLoad());
        }
        public IEnumerator initalLoad()
        {
            WorldManager.getInstance().InitializeMetaData();
            Coroutine itemLoad = StartCoroutine(ItemRegistry.LoadItems());
            Coroutine recipeLoad = StartCoroutine(RecipeRegistry.LoadRecipes());
            yield return itemLoad;
            yield return recipeLoad;
            
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            playerScript.Initialize();
            
            ItemCatalogueController catalogueControllers = GameObject.FindObjectOfType<ItemCatalogueController>();
            catalogueControllers.ShowAll();
            
            WorldManager.getInstance().InitializeQuestBook();

            string path = WorldLoadUtils.GetCurrentWorldPath();
            Debug.Log($"Loading world from path {path}");

            softLoadSystems();
            PlayerIO[] players = GameObject.FindObjectsOfType<PlayerIO>();
            foreach (PlayerIO player in players) {
                DimController dimController = getDimController(0);
                playerWorldData[player.transform] = new PlayerWorldData(null,null,null);
                int dim = player.playerData.dim;
                Vector2 position = new Vector2(player.playerData.x,player.playerData.y);
                setPlayerSystem(player.transform,0,Vector2Int.zero);
            }
        }
    

        public abstract void softLoadSystems();

        public ClosedChunkSystem getPlayerSystem(Transform player) {
            if (!playerWorldData.TryGetValue(player, out var value)) {
                return null;
            }
            return value.closedChunkSystem;
        }
        public int getPlayerDimension(Transform transform) {
            if (playerWorldData.ContainsKey(transform)) {
                return playerWorldData[transform].closedChunkSystem.Dim;
            }
            Debug.LogError($"Player {transform.name} was not in playerDimension Dict");
            return 0;
        }
        public void setPlayerSystem(Transform player, int dim, Vector2Int teleportPosition, IDimensionTeleportKey key = null) {
            DimController controller = getDimController(dim);
            ClosedChunkSystem newSystem = null;
            Vector2Int systemPosition = GetNextSystemPosition();
            Vector2Int offset = systemPosition*DimensionUtils.ACTIVE_SYSTEM_SIZE;

            if (controller is ISingleSystemController singleSystemController) {
                newSystem = singleSystemController.getActiveSystem();
                if (newSystem == null) {
                    newSystem = singleSystemController.activateSystem(offset);
                }
            } else if (controller is IMultipleSystemController multipleSystemController) {
                newSystem = multipleSystemController.getActiveSystem(key);
                if (newSystem == null) {
                    newSystem = multipleSystemController.activateSystem(key,offset);
                }
            }
            if (newSystem == null) {
                Debug.LogError("Could not switch player system");
                return;
            }

            if (playerWorldData.ContainsKey(player) && newSystem.Equals(playerWorldData[player].closedChunkSystem))
            {
                player.transform.position = Vector2.zero;
                return;
            }
            newSystem.initalizeMiscObjects(miscObjects);

            
            if (playerWorldData.ContainsKey(player)) {
                ClosedChunkSystem previousSystem = playerWorldData[player].closedChunkSystem;
                playerWorldData[player].closedChunkSystem = newSystem;
                bool systemEmpty = true;
                if (previousSystem != null) {
                    foreach (KeyValuePair<Transform,PlayerWorldData> kvp in playerWorldData) {
                        if (previousSystem.Equals(kvp.Value.closedChunkSystem)) {
                            systemEmpty = false;
                            break;
                        }
                    }
                    activeSystems.Remove(previousSystem);
                    if (systemEmpty) {
                        previousSystem.deactivateAllPartitions();
                        GameObject.Destroy(previousSystem.gameObject);
                    }
                }
            }
            activeSystems[newSystem] = systemPosition;
            Vector2Int systemOffset = systemPosition*DimensionUtils.ACTIVE_SYSTEM_SIZE;
            playerWorldData[player].chunkPos = null;
            playerWorldData[player].partitionPos = null;
            BackgroundImageController.Instance?.setOffset(new Vector2(
                -systemOffset.x/2f,
                -systemOffset.y/2f
            ));
            
            Vector2Int tpPosition = (teleportPosition-systemOffset);
            Vector3 playerPosition = player.transform.position;
            playerPosition.x = tpPosition.x/2f;
            playerPosition.y = tpPosition.y/2f;
            player.transform.position = playerPosition;
            CanvasController.Instance.ClearStack();
            newSystem.instantCacheChunksNearPlayer();
            newSystem.playerPartitionUpdate();
        }
        
        protected Vector2Int GetNextSystemPosition() {
            int count = activeSystems.Count;
            if (count == 0) {
                return Vector2Int.zero;
            }
            int x = count / 2;
            if (count % 2 == 1) {
                x *= -1;
            }
            return new Vector2Int(x,0);
        }

        private async Task unloadUnusedAssets(ClosedChunkSystem system) {
            EntityRegistry entityRegistry = EntityRegistry.getInstance();
            entityRegistry.reset();
            await entityRegistry.cacheFromSystem(system);
        }

        public abstract DimController getDimController(int dim);

        private class PlayerWorldData {
            public Vector2Int? chunkPos;
            public Vector2Int? partitionPos;
            public ClosedChunkSystem closedChunkSystem;

            public PlayerWorldData(Vector2Int? chunkPos, Vector2Int? partitionPos, ClosedChunkSystem closedChunkSystem)
            {
                this.chunkPos = chunkPos;
                this.partitionPos = partitionPos;
                this.closedChunkSystem = closedChunkSystem;
            }
        }
    }
}
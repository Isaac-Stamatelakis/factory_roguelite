using System;
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
        private int ticksSinceLastSave = 40;
        private const int AUTO_SAVE_TIME = 50 * 300; // One per 5 minutes 
        [SerializeField] private DimensionObjects miscObjects;
        public MiscDimAssets MiscDimAssets;
        private static DimensionManager instance;
        public static DimensionManager Instance {get => instance;}
        public void Awake() {
            instance = this;
        }
        protected DimController currentDimension;
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        private ClosedChunkSystem activeSystem;
        public void Start() {
            StartCoroutine(InitalLoad());
        }
        public IEnumerator InitalLoad()
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

            SoftLoadSystems();
            SetPlayerSystem(playerScript,0,Vector2Int.zero);
        }

        protected abstract List<DimController> GetAllControllers();

        public void FixedUpdate()
        {
            if (!activeSystem) return;
            ticksSinceLastSave++;
            if (ticksSinceLastSave < AUTO_SAVE_TIME) return;
            ticksSinceLastSave = 0;
            StartCoroutine(AutoSaveCoroutine());
        }

        private IEnumerator AutoSaveCoroutine()
        {
            List<DimController> controllers = GetAllControllers();
            Debug.Log("Auto Saving Started");
            int systems = 0;
            foreach (DimController controller in controllers)
            {
                if (controller is ISingleSystemController singleSystemController)
                {
                    yield return StartCoroutine(singleSystemController.SaveSystem());
                    systems++;
                } else if (controller is IMultipleSystemController multipleSystemController)
                {
                    foreach (SoftLoadedClosedChunkSystem system in multipleSystemController.GetAllInactiveSystems())
                    {
                        systems++;
                        yield return StartCoroutine(system?.SaveCoroutine());
                    }
                }
            }
            Debug.Log($"Saved {systems} systems.");
        }


        public abstract void SoftLoadSystems();

        public ClosedChunkSystem GetPlayerSystem()
        {
            return activeSystem;
        }
        public int GetPlayerDimension()
        {
            return activeSystem?.Dim ?? int.MinValue;
        }

        private ClosedChunkSystem GetControllerSystem(DimController controller, PlayerScript playerScript, IDimensionTeleportKey key = null)
        {
            switch (controller)
            {
                case ISingleSystemController singleSystemController:
                {
                    var system = singleSystemController.GetActiveSystem();
                    if (!system)
                    {
                        system = singleSystemController.ActivateSystem(playerScript);
                    }
                    return system;
                }
                case IMultipleSystemController multipleSystemController:
                {
                    var system = multipleSystemController.GetActiveSystem(key);
                    if (!system)
                    {
                        system = multipleSystemController.ActivateSystem(key, playerScript);
                    }
                    return system;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void SetPlayerSystem(PlayerScript player, int dim, Vector2Int teleportPosition, IDimensionTeleportKey key = null) {
            DimController controller = GetDimController(dim);
            ClosedChunkSystem newSystem = GetControllerSystem(controller, player, key);
            
            if (!newSystem) {
                Debug.LogError("Could not switch player system");
                return;
            }
            player.PlayerRobot.TemporarilyPausePlayer();
            
            if (ReferenceEquals(newSystem,activeSystem))
            {
                player.transform.position = Vector2.zero;
                return;
            }
            
            if (newSystem is not ConduitTileClosedChunkSystem && activeSystem is ConduitTileClosedChunkSystem conduitTileClosedChunkSystem)
            {
                player.TileViewers.DisableConduitViewers();
            }
            else
            {
                player.TileViewers.ConduitPortViewer.enabled = true;
            }
            
            if (!ReferenceEquals(activeSystem,null) && !ReferenceEquals(activeSystem, newSystem))
            {
                activeSystem.DeactivateAllPartitions();
                GameObject.Destroy(activeSystem.gameObject);
            }
            
            activeSystem = newSystem;
            newSystem.InitalizeMiscObjects(miscObjects);
            BackgroundImageController.Instance?.setOffset(Vector2.zero);
            
            Vector3 playerPosition = player.transform.position;
            
            playerPosition.x = teleportPosition.x*Global.TILE_SIZE;
            playerPosition.y = teleportPosition.y*Global.TILE_SIZE;
            player.transform.position = playerPosition;
            
            CanvasController.Instance.ClearStack();
            
            newSystem.InstantCacheChunksNearPlayer();
            newSystem.PlayerPartitionUpdate();
        }

        public abstract DimController GetDimController(int dim);
    }
}
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
using Newtonsoft.Json;
using Player;
using Player.Tool;
using RecipeModule;
using PlayerModule;
using Recipe;
using TileEntity;
using UI;
using UI.JEI;
using UI.QuestBook;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using World.BackUp;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Dimensions {
    public interface ICompactMachineDimManager {
        public CompactMachineDimController GetCompactMachineDimController();
    }
    public abstract class DimensionManager : MonoBehaviour
    {
        [SerializeField] private AssetReference AutoSavePrefabRef;
        private int ticksSinceLastSave;
        private const int AUTO_SAVE_TIME = 50 * 300;
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
            Coroutine itemLoad = StartCoroutine(ItemRegistry.LoadItems());
            Coroutine recipeLoad = StartCoroutine(RecipeRegistry.LoadRecipes());
            yield return itemLoad;
            yield return recipeLoad;
            
            string path = WorldLoadUtils.GetCurrentWorldPath();
            Debug.Log($"Loading world from path {path}");
            
            ItemCatalogueController catalogueControllers = GameObject.FindObjectOfType<ItemCatalogueController>();
            catalogueControllers.ShowAll();
            
            WorldManager worldManager = WorldManager.getInstance();
            if (!TryExecuteInitialLoad(worldManager.InitializeMetaData, null, "MetaData")) yield break;
            if (!TryExecuteInitialLoad(worldManager.InitializeQuestBook,null, "QuestBook")) yield break;
            
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            if (!TryExecuteInitialLoad(playerScript.Initialize,null, "Player")) yield break;
            
            
            if (!TryExecuteInitialLoad(SoftLoadSystems,null,"SoftLoad")) yield break;
            
            SetPlayerSystem(playerScript, 0, Vector2Int.zero);
            WorldBackUpUtils.CleanUpBackups(worldManager.GetWorldName());
            WorldBackUpUtils.BackUpWorld(worldManager.GetWorldName());
     
        }
        

        private bool TryExecuteInitialLoad(Action action, Action errorAction, string loadName)
        {
            #if UNITY_EDITOR
            action.Invoke();
            return true;
            #endif
            try
            {
                action.Invoke();
                return true;
            }
            catch (Exception e) when (e is NullReferenceException or ArgumentNullException or JsonSerializationException or FileNotFoundException)
            {
                Debug.LogError($"World failed to load at stage {loadName}: '{e.Message}'");
                SceneManager.LoadScene("TitleScreen");
                return false;
            }
        }

        protected abstract List<DimController> GetAllControllers();

        public void FixedUpdate()
        {
            bool canAutoBackup = activeSystem && WorldLoadUtils.UsePersistentPath;
            if (!canAutoBackup) return;
            ticksSinceLastSave++;
            if (ticksSinceLastSave < AUTO_SAVE_TIME) return;
            StartCoroutine(AutoSaveCoroutine());
            ticksSinceLastSave = int.MinValue; // Will be set to 0 once coroutine done
        }

        private IEnumerator AutoSaveCoroutine()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(AutoSavePrefabRef);
            yield return handle;
            GameObject autoSavePrefab = handle.Result;
            Canvas canvas = CanvasController.Instance.GetComponentInParent<Canvas>();
            AutoSaveUI autoSaveUI = GameObject.Instantiate(autoSavePrefab,canvas.transform,false).GetComponent<AutoSaveUI>();
            
            yield return StartCoroutine(autoSaveUI.DisplayCountdown());
            List<DimController> controllers = GetAllControllers();
            Debug.Log("Auto Saving Started");
            int systems = 0;
            foreach (DimController controller in controllers)
            {
                if (controller is ISingleSystemController singleSystemController)
                {
                    yield return StartCoroutine(singleSystemController.SaveSystemCoroutine());
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
            ticksSinceLastSave = 0;
            StartCoroutine(autoSaveUI.CompletionFade());
            WorldBackUpUtils.BackUpWorld(WorldManager.getInstance().GetWorldName());
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

        public void OnDestroy()
        {
            bool loadedSuccessfully = activeSystem;
            if (!loadedSuccessfully) return;
            List<DimController> controllers = GetAllControllers();
            foreach (DimController controller in controllers)
            {
                if (controller is ISingleSystemController singleSystemController)
                {
                    singleSystemController.SaveSystem();
                } else if (controller is IMultipleSystemController multipleSystemController)
                {
                    foreach (SoftLoadedClosedChunkSystem system in multipleSystemController.GetAllInactiveSystems())
                    {
                        system.Save(SerializationMode.Standard);
                    }
                }
            }

            
            if (WorldLoadUtils.UsePersistentPath)
            {
                WorldManager.getInstance().SaveMetaData();
                WorldManager.getInstance().SaveQuestBook();
                WorldBackUpUtils.BackUpWorld(WorldManager.getInstance().GetWorldName());
            }
            
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
                if (currentDimension)
                {
                    currentDimension.ClearEntities();
                }
            }
            
            currentDimension = controller;
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
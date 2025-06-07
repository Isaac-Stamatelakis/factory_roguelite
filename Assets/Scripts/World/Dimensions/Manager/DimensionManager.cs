using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using PlayerModule.IO;
using WorldModule;
using Chunks.Systems;
using Entities.Mobs;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;
using Chunks;
using Tiles;
using Items;
using Misc.Audio;
using Newtonsoft.Json;
using Player;
using Player.Tool;
using RecipeModule;
using PlayerModule;
using Recipe;
using TileEntity;
using TileMaps;
using Tiles.TileMap.Interval;
using UI;
using UI.GraphicSettings;
using UI.Indicators;
using UI.Indicators.General;
using UI.JEI;
using UI.QuestBook;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using World;
using World.BackUp;
using World.Cave.Registry;
using World.Dimensions.Serialization;
using World.Serialization;
using WorldModule.Caves;
using Debug = UnityEngine.Debug;
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
        private bool setPlayerSystemRoutineActive;
        public DimController CurrentDimension { get => currentDimension; set => currentDimension = value; }
        protected ClosedChunkSystem activeSystem;
        
        public void Start() {
            StartCoroutine(InitialLoad());
        }

        private IEnumerator InitialLoad()
        {
            Canvas[] canvasArray = GameObject.FindObjectsOfType<Canvas>();
            PlayerScript playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
            InitialLoadBeautifier loadBeautifier = new InitialLoadBeautifier(canvasArray, playerScript);
            loadBeautifier.Hide();
            
            Coroutine itemLoad = StartCoroutine(ItemRegistry.LoadItems());
            Coroutine entityInitialize = StartCoroutine(EntityRegistry.Initialize());
            yield return itemLoad;
            yield return entityInitialize;
            
            // Recipes must be initialized after items :(
            Coroutine recipeLoad = StartCoroutine(RecipeRegistry.LoadRecipes());
            yield return recipeLoad;
            
            string path = WorldLoadUtils.GetCurrentWorldPath();
            Debug.Log($"Loading world from path {path}");
            
            WorldManager worldManager = WorldManager.GetInstance();
            string worldName = worldManager.GetWorldName();

            void InlineLoadQuestBook()
            {
                WorldLoadUtils.InitializeQuestBook(worldName);
            }
            if (!TryExecuteInitialLoad(InlineLoadQuestBook,null, "QuestBook")) yield break;
            
            PlayerData playerData = null;
            void InlineLoadPlayer()
            {
                playerData = playerScript.Initialize();
            }
            if (!TryExecuteInitialLoad(InlineLoadPlayer, null, "Player")) yield break;

            void InlineLoadMetaData()
            {
                InitializeMetaData(worldManager, playerScript);
            }
            if (!TryExecuteInitialLoad( InlineLoadMetaData, null, "MetaData")) yield break;
            
            if (!TryExecuteInitialLoad(SoftLoadSystems,null,"SoftLoad")) yield break;
            yield return SetPlayerSystem(playerScript, playerData.dimensionData);
            
            
            WorldBackUpUtils.CleanUpBackups(worldManager.GetWorldName());
            WorldBackUpUtils.BackUpWorld(worldManager.GetWorldName());
            
            GraphicSettingsUtils.ApplyWorldGraphicSettings();
            
            playerScript.CallInitializeListeners();
            playerScript.PlayerInventory.Give(ItemSlotFactory.DeserializeSlot(playerData.miscPlayerData?.GrabbedItemData));
            playerScript.PlayerInventory.ChangeSelectedSlot(0);
            
            playerScript.TileViewers.Initialize(playerScript);
            playerScript.GetComponent<PlayerIO>().OnValidated();
            loadBeautifier.Show();
        }

        private void InitializeMetaData(WorldManager worldManager, PlayerScript playerScript)
        {
            WorldMetaData metaData = worldManager.GetMetaData();
            playerScript.Cheats = metaData.CheatsEnabled;
            QuestBookUIManager questBookUIManager = GameObject.FindObjectOfType<QuestBookUIManager>();
            questBookUIManager.Initialize(metaData.QuestBook,playerScript);
        }

        public void OnCaveRegistryLoad(CaveRegistry caveRegistry)
        {
            foreach (DimController controller in GetAllControllers())
            {
                if (controller is ISingleSystemController singleSystemController)
                {
                    IChunkSystem chunkSystem = singleSystemController.GetSystem();
                    chunkSystem?.SyncCaveRegistryTileEntities(caveRegistry);
                } else if (controller is IMultipleSystemController multipleSystemController)
                {
                    foreach (IChunkSystem system in multipleSystemController.GetAllSystems())
                    {
                        system.SyncCaveRegistryTileEntities(caveRegistry);
                    }
                }
            }
        }
        
#pragma warning disable 0162
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
#pragma warning restore 0162
        protected abstract List<DimController> GetAllControllers();

        public void FixedUpdate()
        {
            if (!activeSystem) return;
            BackUpUpdate();
            TickUpdate();
        }

        protected abstract void TickUpdate();

        private void BackUpUpdate()
        {
            if (!WorldLoadUtils.UsePersistentPath) return;
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
                    yield return singleSystemController.GetSystem()?.SaveCoroutine();
                    systems++;
                } else if (controller is IMultipleSystemController multipleSystemController)
                {
                    foreach (IChunkSystem system in multipleSystemController.GetAllSystems())
                    {
                        systems++;
                        yield return StartCoroutine(system?.SaveCoroutine());
                    }
                }
            }
            PlayerManager.Instance.GetPlayer().GetComponent<PlayerIO>().Serialize();
            Debug.Log($"Saved {systems} systems.");
            ticksSinceLastSave = 0;
            StartCoroutine(autoSaveUI.CompletionFade());
            WorldBackUpUtils.BackUpWorld(WorldManager.GetInstance().GetWorldName());
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
        
        public Dimension? GetPlayerDimensionType()
        {
            if (!activeSystem) return null;
            return (Dimension)activeSystem.Dim;
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
                    singleSystemController.GetSystem()?.Save();
                } else if (controller is IMultipleSystemController multipleSystemController)
                {
                    foreach (IChunkSystem system in multipleSystemController.GetAllSystems())
                    {
                        system.Save();
                    }
                }
            }

            if (!WorldLoadUtils.UsePersistentPath) return;
            
            WorldManager.GetInstance().SaveMetaData();
            WorldBackUpUtils.BackUpWorld(WorldManager.GetInstance().GetWorldName());
        }

        private ClosedChunkSystem GetControllerSystem(DimController controller, PlayerScript playerScript, IDimensionTeleportKey key = null)
        {
            try
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
                        var system = multipleSystemController.GetActiveSystem();
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
            catch (InvalidSystemException invalidSystemException)
            {
                Debug.LogError(invalidSystemException.Message);
                return null;
            }
        }

        public IEnumerator SetPlayerSystem(PlayerScript playerScript, PlayerDimensionData playerDimensionData)
        {
            DimensionData dimensionData = playerDimensionData?.DimensionData;
            if (dimensionData == null)
            {
                SetDefaultPlayerSystem(playerScript);
                yield break;
            }

            IDimensionTeleportKey key = null;
            
            Dimension dimension = (Dimension)dimensionData.Dim;
            DimensionOptions dimensionOptions = null;
            switch (dimension)
            {
                case Dimension.OverWorld:
                    break;
                case Dimension.Cave:
                    var handle = Addressables.LoadAssetsAsync<CaveObject>("cave",null);
                    yield return handle;
                    var result = handle.Result;
                    var allCaves = new List<CaveObject>();
                    foreach (CaveObject cave in result)
                    {
                        allCaves.Add(cave);
                    }
                    Addressables.Release(handle);
                    CaveObject caveObject = null;
                    SerializedCaveDimData serializedCaveDimData;
                    try
                    {
                        serializedCaveDimData = JsonConvert.DeserializeObject<SerializedCaveDimData>(dimensionData.DimData);
                    }
                    catch (JsonReaderException)
                    {
                        SetDefaultPlayerSystem(playerScript);
                        yield break;
                    }
                    
                    string caveId = serializedCaveDimData.CaveId;
                    foreach (CaveObject cave in allCaves)
                    {
                        if (cave.GetId() == caveId)
                        {
                            caveObject = cave;
                            break;
                        }
                    }

                    if (!caveObject)
                    {
                        SetDefaultPlayerSystem(playerScript);
                        yield break;
                    }
                    CaveController caveController = (CaveController)GetDimController(Dimension.Cave);
                    caveController.setCurrentCave(caveObject,new Vector2(serializedCaveDimData.X,serializedCaveDimData.Y));
                    dimensionOptions = new DimensionOptions(caveObject.CaveOptions);
                    break;
                case Dimension.CompactMachine:
                    try
                    {
                        key = JsonConvert.DeserializeObject<CompactMachineTeleportKey>(dimensionData.DimData);
                    }
                    catch (Exception e) when (e is NullReferenceException or JsonSerializationException or JsonReaderException)
                    {
                        SetDefaultPlayerSystem(playerScript);
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            yield return SetPlayerSystemCoroutine(playerScript,dimension,new Vector2(playerDimensionData.X,playerDimensionData.Y),key,dimensionOptions);
            
        }

        private void SetDefaultPlayerSystem(PlayerScript playerScript)
        {
            SetPlayerSystem(playerScript,Dimension.OverWorld,Vector2Int.zero);
        }

        public void SetPlayerSystem(PlayerScript player, Dimension dimension, Vector2 teleportPosition, IDimensionTeleportKey key = null, DimensionOptions dimensionOptions = null)
        {
            if (setPlayerSystemRoutineActive)
            {
                Debug.LogWarning("Cannot initiate dimension switch for player as another switch is already in progress");
                return;
            }
            StartCoroutine(SetPlayerSystemCoroutine(player, dimension, teleportPosition, key, dimensionOptions));
        }

        private IEnumerator SetPlayerSystemCoroutine(PlayerScript player, Dimension dimension, Vector2 teleportPosition, IDimensionTeleportKey key = null, DimensionOptions dimensionOptions = null)
        {
            if (setPlayerSystemRoutineActive)
            {
                Debug.LogWarning("Cannot initiate dimension switch for player as another switch is already in progress");
                yield break;
            }
            setPlayerSystemRoutineActive = true;
            
            DimController controller = GetDimController(dimension);
            if (player.PlayerRobot.Dead)
            {
                controller = GetDimController(Dimension.OverWorld);
                player.PlayerRobot.Heal(9999);
            }
            if (activeSystem && activeSystem.Dim == (int)dimension && controller is ISingleSystemController)
            {
                player.transform.position = teleportPosition;
                setPlayerSystemRoutineActive = false;
                yield break;
            }
            
            dimensionOptions ??= GetDimensionOptions(dimension);
            
            activeSystem?.Save();
            activeSystem?.DeactivateAllPartitions();
            currentDimension?.DeActivateSystem();
            
            ClosedChunkSystem newSystem = GetControllerSystem(controller, player, key);
            if (!newSystem) {
                Debug.LogError("Could not switch player system");
                yield break;
            }
            player.PlayerRobot.TemporarilyPausePlayer();
            
            
            if (ReferenceEquals(newSystem,activeSystem))
            {
                player.transform.position = teleportPosition;
                setPlayerSystemRoutineActive = false;
                yield break;
            }
            
            if (newSystem is not ConduitClosedChunkSystem && activeSystem is ConduitClosedChunkSystem)
            {
                player.TileViewers.DisableConduitViewers();
            }
            else
            {
                player.TileViewers.ConduitPortViewer.enabled = true;
                
            }
            
            currentDimension = controller;
            activeSystem = newSystem;
            newSystem.InitializeMiscObjects(miscObjects);
            BackgroundImageController.Instance?.setOffset(Vector2.zero);
            
            IntervalVector coveredArea = activeSystem.CoveredArea;
            const int BONUS_MIN = 1;
            const float CHUNK_WORLD_SIZE = Global.CHUNK_SIZE * Global.TILE_SIZE;
            teleportPosition.x = Mathf.Clamp(teleportPosition.x,coveredArea.X.LowerBound*CHUNK_WORLD_SIZE+BONUS_MIN,(coveredArea.X.UpperBound+1)*CHUNK_WORLD_SIZE-BONUS_MIN);
            teleportPosition.y = Mathf.Clamp(teleportPosition.y,coveredArea.Y.LowerBound*CHUNK_WORLD_SIZE+BONUS_MIN,(coveredArea.Y.UpperBound+1)*CHUNK_WORLD_SIZE-BONUS_MIN);
            
            
            
            player.SyncToClosedChunkSystem(newSystem,this,dimension);
            player.DimensionData = PlayerDimensionDataFactory.SerializeDimensionData(this);
            GlobalHelper.DeleteAllChildren(player.TemporaryObjectContainer);
            
            CanvasController.Instance.ClearStack();
            
            player.SetParticles(dimensionOptions.ParticleOptions);
            Light2D light2D = GameObject.FindWithTag("GlobalLight").GetComponent<Light2D>();
            light2D.intensity = dimensionOptions.LightIntensity;
            if (DevMode.Instance.LightOn && light2D.intensity < 1)
            {
                light2D.intensity = 1;
            }
            light2D.color = dimensionOptions.LightColor;
            
            BlockWorldTileMap[] outlineTileGridMaps = FindObjectsOfType<BlockWorldTileMap>();
            foreach (BlockWorldTileMap outlineTileGridMap in outlineTileGridMaps) {
                outlineTileGridMap.SetView(false,dimensionOptions.OutlineColor);
            }

            if (dimensionOptions.DefaultSong)
            {
                MusicTrackController.Instance.RestoreDefaultSong();
            }
            
            player.PlayerUIContainer.IndicatorManager.Display(player);
            yield return newSystem.LoadTileEntityAssets();
            
            Vector3 playerPosition = player.transform.position;
            playerPosition.x = teleportPosition.x;
            playerPosition.y = teleportPosition.y;
            player.transform.position = playerPosition;
                
            newSystem.InstantCacheChunksNearPlayer();
            newSystem.PlayerPartitionUpdate();
            
            yield return new WaitForSeconds(0.1f); // Prevent players from spamming teleport
            
            setPlayerSystemRoutineActive = false;
        }
        
        
        public abstract DimController GetDimController(Dimension dimension);

        private DimensionOptions GetDimensionOptions(Dimension dimension)
        {
            switch (dimension)
            {
                case Dimension.Cave: // This is probably not required for cave
                case Dimension.OverWorld:
                    return new DimensionOptions(Color.white, Color.black, 0.05f, null,true);
                case Dimension.CompactMachine:
                    return new DimensionOptions(Color.white, Color.black, 0.05f, null,true);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private class InitialLoadBeautifier
        {
            private readonly Canvas[] canvasArray;
            private readonly SpriteRenderer playerSpriteRenderer;
            private readonly SpriteRenderer[] playerSubSpriteRenderers;

            public InitialLoadBeautifier(Canvas[] canvasArray, PlayerScript playerScript)
            {
                this.canvasArray = canvasArray;
                this.playerSpriteRenderer = playerScript.GetComponent<SpriteRenderer>();
                playerSubSpriteRenderers = playerScript.GetComponentsInChildren<SpriteRenderer>();
            }


            private void SetState(bool enabled)
            {
                foreach (Canvas canvas in canvasArray)
                {
                    canvas.enabled = enabled;
                }
                playerSpriteRenderer.enabled = enabled;
                foreach (SpriteRenderer spriteRenderer in playerSubSpriteRenderers)
                {
                    if (enabled && spriteRenderer.tag == "RobotArm") continue; // Don't re-enable gun arm
                    spriteRenderer.enabled = enabled;
                }
            }
        
            public void Hide()
            {
                SetState(false);
            }

            public void Show()
            {
                SetState(true);
            }
        }
    }

    public class DimensionOptions
    {
        public Color LightColor;
        public Color OutlineColor;
        public float LightIntensity;
        public ParticleOptions ParticleOptions;
        public bool DefaultSong;

        public DimensionOptions(Color lightColor, Color outlineColor, float lightIntensity, ParticleOptions particleOptions, bool useDefaultSong)
        {
            LightColor = lightColor;
            OutlineColor = outlineColor;
            LightIntensity = lightIntensity;
            ParticleOptions = particleOptions;
            DefaultSong = useDefaultSong;
        }

        public DimensionOptions(CaveOptions caveOptions)
        {
            LightColor = caveOptions.LightColor;
            OutlineColor = caveOptions.OutlineColor;
            LightIntensity = caveOptions.LightIntensity;
            ParticleOptions = new ParticleOptions(caveOptions.ParticleColor);
            DefaultSong = false;
        }
    }

    public class ParticleOptions
    {
        public Color ParticleColor;

        public ParticleOptions(Color particleColor)
        {
            ParticleColor = particleColor;
        }
    }
}
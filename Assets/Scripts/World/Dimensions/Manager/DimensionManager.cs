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
using UI;
using UI.JEI;
using UI.QuestBook;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using World.BackUp;
using World.Cave.Registry;
using World.Dimensions.Serialization;
using World.Serialization;
using WorldModule.Caves;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Dimensions {
    public enum DimensionType
    {
        Cave = -1,
        BaseDim = 0,
        CompactMachines = 1,
    }
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
        protected ClosedChunkSystem activeSystem;
        
        public void Start() {
            StartCoroutine(InitalLoad());
        }
        public IEnumerator InitalLoad()
        {
            Coroutine itemLoad = StartCoroutine(ItemRegistry.LoadItems());
            Coroutine recipeLoad = StartCoroutine(RecipeRegistry.LoadRecipes());
            Coroutine entityInitialize = StartCoroutine(EntityRegistry.Initialize());
            yield return itemLoad;
            yield return recipeLoad;
            yield return entityInitialize;
            
            string path = WorldLoadUtils.GetCurrentWorldPath();
            Debug.Log($"Loading world from path {path}");
            
            WorldManager worldManager = WorldManager.getInstance();
            string worldName = worldManager.GetWorldName();

            void InlineLoadQuestBook()
            {
                WorldLoadUtils.InitializeQuestBook(worldName);
            }
            if (!TryExecuteInitialLoad(InlineLoadQuestBook,null, "QuestBook")) yield break;
            
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            PlayerDimensionData playerDimensionData = null;
            void InlineLoadPlayer()
            {
                playerDimensionData = playerScript.Initialize();
            }
            if (!TryExecuteInitialLoad(InlineLoadPlayer, null, "Player")) yield break;

            void InlineLoadMetaData()
            {
                InitializeMetaData(worldManager, playerScript);
            }
            if (!TryExecuteInitialLoad( InlineLoadMetaData, null, "MetaData")) yield break;
            
            if (!TryExecuteInitialLoad(SoftLoadSystems,null,"SoftLoad")) yield break;
            yield return SetPlayerSystem(playerScript, playerDimensionData);
            WorldBackUpUtils.CleanUpBackups(worldManager.GetWorldName());
            WorldBackUpUtils.BackUpWorld(worldManager.GetWorldName());
            
            ItemCatalogueController catalogueControllers = GameObject.FindObjectOfType<ItemCatalogueController>();
            catalogueControllers.ShowAll();
        }

        private void InitializeMetaData(WorldManager worldManager, PlayerScript playerScript)
        {
            WorldMetaData metaData = worldManager.GetMetaData();
            playerScript.Cheats = metaData.CheatsEnabled;
            QuestBookUIManager questBookUIManager = GameObject.FindObjectOfType<QuestBookUIManager>();
            questBookUIManager.Initialize(metaData.QuestBook);
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
            
            WorldManager.getInstance().SaveMetaData();
            WorldBackUpUtils.BackUpWorld(WorldManager.getInstance().GetWorldName());
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
                    foreach (CaveObject cave in allCaves)
                    {
                        if (cave.GetId() == dimensionData.DimData)
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
                    caveController.setCurrentCave(caveObject);
                    dimensionOptions = new DimensionOptions(caveObject.CaveOptions);
                    break;
                case Dimension.CompactMachine:
                    try
                    {
                        key = JsonConvert.DeserializeObject<CompactMachineTeleportKey>(dimensionData.DimData);
                    }
                    catch (Exception e) when (e is NullReferenceException or JsonSerializationException)
                    {
                        SetDefaultPlayerSystem(playerScript);
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            SetPlayerSystem(playerScript,dimension,new Vector2(playerDimensionData.X,playerDimensionData.Y),key,dimensionOptions);
            
        }

        private void SetDefaultPlayerSystem(PlayerScript playerScript)
        {
            SetPlayerSystem(playerScript,Dimension.OverWorld,Vector2Int.zero);
        }
        

        public void SetPlayerSystem(PlayerScript player, Dimension dimension, Vector2 teleportPosition, IDimensionTeleportKey key = null, DimensionOptions dimensionOptions = null) {
            DimController controller = GetDimController(dimension);
            if (activeSystem && activeSystem.Dim == (int)dimension && controller is ISingleSystemController)
            {
                return;
            }
            
            dimensionOptions ??= GetDimensionOptions(dimension);
            activeSystem?.Save();
            activeSystem?.DeactivateAllPartitions();
            currentDimension?.ClearEntities();
            currentDimension?.DeActivateSystem();
            
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
            
            currentDimension = controller;
            activeSystem = newSystem;
            newSystem.InitializeMiscObjects(miscObjects);
            BackgroundImageController.Instance?.setOffset(Vector2.zero);
            
            Vector3 playerPosition = player.transform.position;

            IntervalVector coveredArea = activeSystem.CoveredArea;
            const int BONUS_MIN = 1;
            const float CHUNK_WORLD_SIZE = Global.CHUNK_SIZE * Global.TILE_SIZE;
            teleportPosition.x = Mathf.Clamp(teleportPosition.x,coveredArea.X.LowerBound*CHUNK_WORLD_SIZE+BONUS_MIN,(coveredArea.X.UpperBound+1)*CHUNK_WORLD_SIZE-BONUS_MIN);
            teleportPosition.y = Mathf.Clamp(teleportPosition.y,coveredArea.Y.LowerBound*CHUNK_WORLD_SIZE+BONUS_MIN,(coveredArea.Y.UpperBound+1)*CHUNK_WORLD_SIZE-BONUS_MIN);
            
            playerPosition.x = teleportPosition.x;
            playerPosition.y = teleportPosition.y;
            player.transform.position = playerPosition;
            player.DimensionData = PlayerDimensionDataFactory.SerializeDimensionData(this);
            
            CanvasController.Instance.ClearStack();
            
            player.SetParticles(dimensionOptions.ParticleOptions);
            Light2D light2D = GameObject.FindWithTag("GlobalLight").GetComponent<Light2D>();
            light2D.intensity = dimensionOptions.LightIntensity;
            if (DevMode.Instance.LightOn && light2D.intensity < 1)
            {
                light2D.intensity = 1;
            }
            light2D.color = dimensionOptions.LightColor;
            
            OutlineWorldTileGridMap[] outlineTileGridMaps = FindObjectsOfType<OutlineWorldTileGridMap>();
            foreach (OutlineWorldTileGridMap outlineTileGridMap in outlineTileGridMaps) {
                outlineTileGridMap.setView(false,dimensionOptions.OutlineColor);
            }

            if (dimensionOptions.DefaultSong)
            {
                MusicTrackController.Instance.RestoreDefaultSong();
            }
            
            newSystem.InstantCacheChunksNearPlayer();
            newSystem.PlayerPartitionUpdate();
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
using System;
using System.Collections.Generic;
using System.IO;
using Chunks.Systems;
using Conduit.Placement.LoadOut;
using Conduit.View;
using Conduits;
using Conduits.Ports;
using Conduits.PortViewer;
using Conduits.Systems;
using Dimensions;
using Item.GameStage;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Newtonsoft.Json;
using Player.Controls;
using Player.Movement;
using Player.UI;
using PlayerModule;
using PlayerModule.IO;
using PlayerModule.KeyPress;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.Info;
using Robot.Upgrades.LoadOut;
using Robot.Upgrades.Network;
using RobotModule;
using TileMaps.Previewer;
using Tiles;
using Tiles.Indicators;
using UI;
using UI.Catalogue.ItemSearch;
using UI.QuestBook;
using UI.RingSelector;
using UI.Statistics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using World.Dimensions.Serialization;
using WorldModule;

namespace Player
{
    
    public class PlayerScript : MonoBehaviour
    {
        private PlayerInventory playerInventory;
        private PlayerRobot playerRobot;
        private PlayerIO playerIO;
        private PlayerMouse playerMouse;
        [SerializeField] private ConduitPlacementOptions conduitPlacementOptions;
        [SerializeField] private PlayerTilePlacementOptions tilePlacementOptions;
        [SerializeField] private ConduitViewOptions conduitViewOptions;
        [SerializeField] private PlayerUIPrefabs prefabs;
        [SerializeField] private PlayerUIContainer playerUIContainer;
        [SerializeField] private TileViewerCollection tileViewers;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private PlayerToolSpawnedObjectCollection SpawnedObjectCollection;
        private InputActions inputActions;
        public InputActions InputActions => inputActions;
        public Transform PersistentObjectContainer => SpawnedObjectCollection.Persistent;
        public Transform TemporaryObjectContainer => SpawnedObjectCollection.Temporary;
        private PlayerGameStageCollection gameStageCollection;
        public PlayerGameStageCollection GameStageCollection => gameStageCollection;
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerRobot PlayerRobot => playerRobot;
        public ConduitPlacementOptions ConduitPlacementOptions => conduitPlacementOptions;
        public PlayerTilePlacementOptions TilePlacementOptions => tilePlacementOptions;
        public ConduitViewOptions ConduitViewOptions => conduitViewOptions;
        private QuestBookCache questBookCache;
        public QuestBookCache QuestBookCache => questBookCache;
        public PlayerUIPrefabs Prefabs => prefabs;
        public PlayerUIContainer PlayerUIContainer => playerUIContainer;
        public TileViewerCollection TileViewers => tileViewers;
        public PlayerMouse PlayerMouse => playerMouse;
        private PlayerStatisticCollection playerStatisticCollection;
        public PlayerStatisticCollection PlayerStatisticCollection => playerStatisticCollection;
        private Vector2 lastPosition;
        public bool Cheats;
        public DimensionData DimensionData;
        private ClosedChunkSystem currentSystem;
        public ClosedChunkSystem CurrentSystem => currentSystem;
        [FormerlySerializedAs("ItemCheat")] public bool ItemSearchCheat;
        

        public void SetInputActions(InputActions inputActions)
        {
            this.inputActions = inputActions;
        }

        public void Start()
        {
            PlayerManager.Instance.RegisterPlayer(this);
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerIO = GetComponent<PlayerIO>();
            playerMouse = GetComponent<PlayerMouse>();
        }
        
        public PlayerData Initialize()
        {
            InitializeStages();
            PlayerData playerData = playerIO.Deserialize();
            PlayerIO.VerifyIntegrityOfPlayerData(playerData);
            
            playerStatisticCollection = playerData.playerStatistics;
            playerInventory.Initialize(playerData.sInventoryData);
            
            
            ItemSlot playerRobotItem = ItemSlotFactory.DeserializeSlot(playerData.playerRobot);
            RobotUpgradeLoadOut robotStatLoadOut = RobotUpgradeUtils.DeserializeRobotStatLoadOut(playerData.sRobotLoadOut);
            playerRobot.InitializeRobot(playerRobotItem,robotStatLoadOut);
            
            playerMouse.Initialize();
            playerMouse.SyncTilePlacementCooldown();
            
            playerInventory.InitializeToolDisplay();
            
            conduitPlacementOptions = new ConduitPlacementOptions(playerData.miscPlayerData.ConduitPortPlacementLoadOuts);
            
            tilePlacementOptions = new PlayerTilePlacementOptions();
            tilePlacementOptions.Initilaize();
            
            questBookCache = new QuestBookCache();
            
            playerUIContainer.IndicatorManager.Initialize(this);
            playerUIContainer.TileIndicatorManagerUI.Initialize(this);
            
            ControlUtils.LoadRequiredAndBlocked();
            ControlUtils.InitializeKeyBindings(inputActions);
            
            CanvasController.Instance.SetPlayerScript(this);
            
            OnReachUpgradeChange();
            
            return playerData;
        }

        
        
        public void CallInitializeListeners()
        {
            IPlayerStartupListener[] playerStartupListeners = GetComponents<IPlayerStartupListener>();
            foreach (IPlayerStartupListener playerStartupListener in playerStartupListeners)
            {
                playerStartupListener.OnInitialized();
            }

            playerStartupListeners = GetComponentsInChildren<IPlayerStartupListener>();
            foreach (IPlayerStartupListener playerStartupListener in playerStartupListeners)
            {
                playerStartupListener.OnInitialized();
            }
        }
        public void SyncKeyPressListeners(bool uiActive, bool typing, bool blockMovement)
        {
            GetComponent<PlayerKeyPress>().SyncEventsWithUIMode(uiActive || typing);
            playerRobot.SetMovementEventListenerState(!typing && !blockMovement);  
        }

        public void SyncToClosedChunkSystem(ClosedChunkSystem closedChunkSystem, DimensionManager dimensionManager, Dimension dimension)
        {
            currentSystem = closedChunkSystem;
            playerMouse.SyncToClosedChunkSystem(closedChunkSystem);
            if (dimension == Dimension.Cave)
            {
                CaveController caveController = (CaveController)dimensionManager.GetDimController(dimension);
                PlayerUIContainer.IndicatorManager.caveIndicatorUI.SyncToSystem(this,caveController.ReturnPortalLocation);
            }

            PlayerFluidCollider playerFluidCollider = GetComponentInChildren<PlayerFluidCollider>();
            playerFluidCollider.FluidTileMap = closedChunkSystem.GetFluidTileMap();
        }

        private void InitializeStages()
        {
            string path = WorldLoadUtils.GetWorldComponentPath(WorldFileType.GameStage);
            if (!File.Exists(path))
            {
                WorldCreation.InititalizeGameStages(path);
            }

            HashSet<string> gameStages = GlobalHelper.DeserializeCompressedJson<HashSet<string>>(path);
            gameStageCollection = new PlayerGameStageCollection(gameStages);
        }

        public void OnReachUpgradeChange()
        {
            playerMouse.SetRange(RobotUpgradeUtils.BASE_REACH + RobotUpgradeUtils.GetContinuousValue(playerRobot.RobotUpgradeLoadOut.SelfLoadOuts,(int)RobotUpgrade.Reach));
        }

        public void FixedUpdate()
        {
            if (playerStatisticCollection == null) return;
            playerStatisticCollection.ContinuousValues[PlayerStatistic.Play_Time] += Time.fixedDeltaTime;
            float distance = Vector2.Distance(transform.position, lastPosition);
            playerStatisticCollection.ContinuousValues[PlayerStatistic.Distance_Traveled] += distance;
            lastPosition = transform.position;
        }

        public void SetParticles(ParticleOptions particleOptions)
        {
            bool optionsNull = particleOptions == null;
            particles.gameObject.SetActive(!optionsNull);
            if (optionsNull) return;

            var main = particles.main;
            main.startColor = particleOptions.ParticleColor;
        }

        public void PlaceUpdate()
        {
            ItemSlot currentPlayerItem = playerInventory.getSelectedItemSlot();
            if (currentPlayerItem?.itemObject is ConduitItem)
            {
                playerUIContainer.TileIndicatorManagerUI.conduitPlacementModeIndicatorUI.IterateCounter();
            }
            if (!DevMode.Instance.noPlaceCost)
            {
                
                playerInventory.deiterateInventoryAmount();
                
            }
        }
    }

    [System.Serializable]
    public class TileViewerCollection
    {
        public TilePlacePreviewer TilePlacePreviewer;
        public PortViewerController ConduitPortViewer;
        [FormerlySerializedAs("TileBreakHighlighter")] public TileHighlighter tileHighlighter;
        [HideInInspector] public ConduitViewController ConduitViewController;
        
        public void Initialize(PlayerScript playerScript)
        {
            TilePlacePreviewer.Initialize(playerScript);
        }
        
        public void SetPlacePreviewerState(bool state)
        {
            TilePlacePreviewer.gameObject.SetActive(state);
        }

        public void SetConduitPortState(bool state)
        {
            ConduitPortViewer.gameObject.SetActive(state);
        }
        
        public void DisableConduitViewers()
        {
            ConduitPortViewer.DeActivate();
            ConduitPortViewer.enabled = false;
            ConduitViewController = null;
        }

        public void SetAllViewerState(bool state)
        {
            TilePlacePreviewer.gameObject.SetActive(state);
            ConduitPortViewer.gameObject.SetActive(state);
        }

    }
    [System.Serializable]
    public class PlayerUIPrefabs
    {
        public UIRingSelector RingSelectorPrefab;
        public ItemSearchUI ItemSearchUIPrefab;
    }
    [System.Serializable]
    public class ConduitPlacementOptions
    {
        public const int LOADOUTS = 3;
        private ConduitType? lastPlacementType;
        public ConduitPlacementMode PlacementMode;
        private HashSet<Vector2Int> PlacementPositions = new HashSet<Vector2Int>();

        public Dictionary<LoadOutConduitType, IOConduitPortData> ConduitPlacementLoadOuts;
        public bool CanConnect(IConduit conduit)
        {
            switch (PlacementMode)
            {
                case ConduitPlacementMode.Any:
                    return true;
                case ConduitPlacementMode.New:
                    return PlacementPositions.Contains(conduit.GetPosition());
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void UpdatePlacementType(ConduitType conduitType)
        {
            if (lastPlacementType == conduitType) return;
            ResetPlacementRecord();
            lastPlacementType = conduitType; // Important to set this after resetting
        }
        
        public void ResetPlacementRecord()
        {
            lastPlacementType = null;
            PlacementPositions.Clear();
        }
        

        public void AddPlacementPosition(Vector2Int position)
        {
            PlacementPositions.Add(position);
        }

        public ConduitPlacementOptions(Dictionary<LoadOutConduitType, IOConduitPortData> conduitPlacementLoadOuts)
        {
            VerifyLoadOut(conduitPlacementLoadOuts);
            
        }

        private void VerifyLoadOut(Dictionary<LoadOutConduitType, IOConduitPortData> conduitPlacementLoadOuts)
        {

            conduitPlacementLoadOuts ??= new Dictionary<LoadOutConduitType, IOConduitPortData>();

            LoadOutConduitType[] loadOutConduitTypes = System.Enum.GetValues(typeof(LoadOutConduitType)) as LoadOutConduitType[];
            foreach (var loadOutConduitType in loadOutConduitTypes)
            {
                ConduitType conduitType = loadOutConduitType.ToConduitType();
                IOConduitPortData defaultData = ConduitPortFactory.GetDefaultIOPortData(conduitType, EntityPortType.All); // Use EntityPort.All since loadouts modify ALL

                if (conduitPlacementLoadOuts.TryAdd(loadOutConduitType, defaultData)) continue;
                
                IOConduitPortData currentData = conduitPlacementLoadOuts[loadOutConduitType];
                if (currentData.InputData.GetType() != defaultData.InputData.GetType())
                {
                    currentData.InputData = defaultData.InputData;
                }
                if (currentData.OutputData.GetType() != defaultData.OutputData.GetType())
                {
                    currentData.OutputData = defaultData.OutputData;
                }
            }
        
            this.ConduitPlacementLoadOuts = conduitPlacementLoadOuts;
        }
        
        
    }

    public enum PlayerTileRotation
    {
        Degrees0 = 0,
        Degrees90 = 1,
        Degrees180 = 2,
        Degrees270 = 3,
        Auto = 4
    }
    [System.Serializable]
    public class PlayerTilePlacementOptions
    {
        public const string AUTO_PLACEMENT_KEY = "_option_auto_placement";
        public const string TILE_HIGHLIGHT_KEY = "_option_auto_placement";
        
        public bool Indiciator = true;
        public bool AutoPlace = true;
        public PlayerTileRotation Rotation;
        public int State;
        public BaseTileData AutoBaseTileData = new BaseTileData(0,0,false);

        public void Initilaize()
        {
            Indiciator = PlayerPrefs.GetInt(TILE_HIGHLIGHT_KEY) == 1;
            AutoPlace = PlayerPrefs.GetInt(AUTO_PLACEMENT_KEY) == 1;
        }

        public void Serialize()
        {
            PlayerPrefs.SetInt(TILE_HIGHLIGHT_KEY, Indiciator ? 1 : 0);
            PlayerPrefs.SetInt(AUTO_PLACEMENT_KEY, AutoPlace ? 1 : 0);
        }
    }

    public static class PlayerTileRotationExtension
    {
        public static int ToValue(this PlayerTileRotation rotation)
        {
            switch (rotation)
            {
                case PlayerTileRotation.Degrees0:
                case PlayerTileRotation.Auto: // For most purposes auto rotation can be considered 0
                    return 0;
                case PlayerTileRotation.Degrees90:
                    return 1;
                case PlayerTileRotation.Degrees180:
                    return 2;
                case PlayerTileRotation.Degrees270:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rotation), rotation, null);
            }
        }
    }

    public class TilePlacementData
    {
        public PlayerTileRotation Rotation;
        public int State;

        public TilePlacementData(PlayerTileRotation rotation, int state)
        {
            Rotation = rotation;
            State = state;
        }
    }

    public class PlayerGameStageCollection
    {
        public HashSet<string> UnlockedStages;
        public bool HasStage(string stage)
        {
            return UnlockedStages.Contains(stage);
        }
        public bool HasStage(GameStageObject stage)
        {
            return UnlockedStages.Contains(stage?.GetGameStageId());
        }
        public PlayerGameStageCollection(HashSet<string> unlockedStages)
        {
            UnlockedStages = unlockedStages;
        }
    }

    [System.Serializable]
    public class PlayerToolSpawnedObjectCollection
    {
        public Transform Persistent;
        public Transform Temporary;
    }
}

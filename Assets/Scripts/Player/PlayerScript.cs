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
using Player.UI;
using PlayerModule;
using PlayerModule.IO;
using PlayerModule.Mouse;
using Robot.Upgrades;
using Robot.Upgrades.LoadOut;
using RobotModule;
using TileMaps.Previewer;
using Tiles;
using Tiles.Highlight;
using Tiles.Indicators;
using UI.Catalogue.ItemSearch;
using UI.QuestBook;
using UI.RingSelector;
using UI.Statistics;
using UnityEngine;
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
        public Transform PersistentObjectContainer => SpawnedObjectCollection.Persistent;
        public Transform TemporaryObjectContainer => SpawnedObjectCollection.Temporary;
        private PlayerGameStageCollection gameStageCollection;
        public PlayerGameStageCollection GameStageCollection => gameStageCollection;
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerRobot PlayerRobot => playerRobot;
        public PlayerIO PlayerIO => playerIO;
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
            playerRobot.Initialize(playerRobotItem,robotStatLoadOut);
            
            playerInventory.InitializeToolDisplay();
            conduitPlacementOptions = new ConduitPlacementOptions(playerData.miscPlayerData.ConduitPortPlacementLoadOuts);
            tilePlacementOptions = new PlayerTilePlacementOptions();
            questBookCache = new QuestBookCache();
            ControlUtils.LoadBindings();
            playerUIContainer.IndicatorManager.Initialize(this);
            tileViewers.Initialize(this);
            
            OnReachUpgradeChange();

            
            return playerData;
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
                playerUIContainer.IndicatorManager.conduitPlacementModeIndicatorUI.IterateCounter();
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
        public TileBreakIndicator TileBreakIndicator;
        public TilePlacePreviewer TilePlacePreviewer;
        public TileHighlighter TileHighlighter;
        public PortViewerController ConduitPortViewer;
        public TileBreakHighlighter TileBreakHighlighter;
        public TileBreakHighlighter MainBreakHighlighter;
        [HideInInspector] public ConduitViewController ConduitViewController;
        public void SetPlacePreviewerState(bool state)
        {
            TilePlacePreviewer.gameObject.SetActive(state);
        }
        public void SetHighligherState(bool state)
        {
            TileHighlighter.gameObject.SetActive(state);
        }

        public void SetConduitPortState(bool state)
        {
            ConduitPortViewer.gameObject.SetActive(state);
        }

        public void Initialize(PlayerScript playerScript)
        {
            TilePlacePreviewer.Initialize(playerScript);
        }

        public void DisableConduitViewers()
        {
            ConduitPortViewer.DeActivate();
            ConduitPortViewer.enabled = false;
            ConduitViewController = null;
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
        
        public bool Indiciator = true;
        public PlayerTileRotation Rotation;
        public int State;
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

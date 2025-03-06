using System;
using System.Collections.Generic;
using System.IO;
using Chunks.Systems;
using Conduit.View;
using Conduits;
using Conduits.PortViewer;
using Conduits.Systems;
using Item.GameStage;
using Item.Slot;
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
using UI.QuestBook;
using UI.RingSelector;
using UnityEngine;
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
        
        public void Start()
        {
            PlayerManager.Instance.RegisterPlayer(this);
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerIO = GetComponent<PlayerIO>();
            playerMouse = GetComponent<PlayerMouse>();
        }
        
        public void Initialize()
        {
            InitializeStages();
            PlayerData playerData = playerIO.Deserialize();
            playerInventory.Initialize(playerData.sInventoryData);
            
            ItemSlot playerRobotItem = ItemSlotFactory.DeserializeSlot(playerData.playerRobot);
            RobotUpgradeLoadOut robotStatLoadOut = RobotUpgradeUtils.DeserializeRobotStatLoadOut(playerData.sRobotLoadOut);
            playerRobot.Initialize(playerRobotItem,robotStatLoadOut);
            
            playerInventory.InitializeToolDisplay();
            conduitPlacementOptions = new ConduitPlacementOptions();
            tilePlacementOptions = new PlayerTilePlacementOptions();
            questBookCache = new QuestBookCache();
            ControlUtils.LoadBindings();
            playerUIContainer.IndicatorManager.Initialize(this);
            tileViewers.Initialize(this);
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

        
    }

    [System.Serializable]
    public class TileViewerCollection
    {
        public TileBreakIndicator TileBreakIndicator;
        public TilePlacePreviewer TilePlacePreviewer;
        public TileHighlighter TileHighlighter;
        public PortViewerController ConduitPortViewer;
        public TileBreakHighlighter TileBreakHighlighter;
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
        public PlayerInventoryUI PlayerInventoryUIPrefab;
    }
    [System.Serializable]
    public class ConduitPlacementOptions
    {
        private ConduitType? lastPlacementType;
        public ConduitPlacementMode PlacementMode;
        private HashSet<Vector2Int> PlacementPositions = new HashSet<Vector2Int>();

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
    }

    [System.Serializable]
    public class PlayerTilePlacementOptions
    {
        public bool Indiciator = true;
        public int Rotation;
        public int State;
    }

    public class TilePlacementData
    {
        public int Rotation;
        public int State;

        public TilePlacementData(int rotation, int state)
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
}

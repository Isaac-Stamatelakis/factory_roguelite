using System;
using System.Collections.Generic;
using Chunks.Systems;
using Conduits;
using Conduits.Systems;
using Item.Slot;
using PlayerModule;
using PlayerModule.IO;
using UnityEngine;

namespace Player
{
    
    public class PlayerScript : MonoBehaviour
    {
        private PlayerInventory playerInventory;
        private PlayerRobot playerRobot;
        private PlayerIO playerIO;
        [SerializeField] private ConduitPlacementOptions conduitPlacementOptions;
        [SerializeField] private PlayerTilePlacementOptions tilePlacementOptions;
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerRobot PlayerRobot => playerRobot;
        public PlayerIO PlayerIO => playerIO;
        public ConduitPlacementOptions ConduitPlacementOptions => conduitPlacementOptions;
        public PlayerTilePlacementOptions TilePlacementOptions => tilePlacementOptions;
        
        public void Start()
        {
            PlayerManager.Instance.RegisterPlayer(this);
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerIO = GetComponent<PlayerIO>();
        }

        public void Initialize()
        {
            playerIO.Deserialize();
            playerInventory.Initialize();
            ItemSlot playerRobotItem = ItemSlotFactory.DeserializeSlot(playerIO.playerData.playerRobot);
            playerRobot.SetRobot(playerRobotItem);
            playerInventory.InitializeToolDisplay();
            conduitPlacementOptions = new ConduitPlacementOptions();
            tilePlacementOptions = new PlayerTilePlacementOptions();
        }
    }
    [System.Serializable]
    public class ConduitPlacementOptions
    {
        private ConduitType lastPlacementType;
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
            lastPlacementType = conduitType;
            ResetPlacementRecord();
        }
        
        public void ResetPlacementRecord()
        {
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
}

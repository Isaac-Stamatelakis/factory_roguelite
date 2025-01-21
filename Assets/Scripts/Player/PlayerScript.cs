using System;
using Chunks.Systems;
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
        private ClosedChunkSystem currentSystem;
        
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerRobot PlayerRobot => playerRobot;
        public PlayerIO PlayerIO => playerIO;
        public ClosedChunkSystem CurrentSystem => currentSystem;
        
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
            ItemSlot playerRobotItem = ItemSlotFactory.deseralizeItemSlotFromString(playerIO.playerData.playerRobot);
            playerRobot.SetRobot(playerRobotItem);
            playerInventory.InitializeToolDisplay();
        }
    }
}

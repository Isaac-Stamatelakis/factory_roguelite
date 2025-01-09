using System;
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
        
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerRobot PlayerRobot => playerRobot;
        public PlayerIO PlayerIO => playerIO;
        
        public void Start()
        {
            PlayerManager.Instance.RegisterPlayer(this);
            playerInventory = GetComponent<PlayerInventory>();
            playerRobot = GetComponent<PlayerRobot>();
            playerIO = GetComponent<PlayerIO>();
        }

        public void Initialize()
        {
            
        }

        
            
    }
}

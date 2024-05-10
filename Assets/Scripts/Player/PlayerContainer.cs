using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerModule {
    public class PlayerContainer
    {
        private static PlayerContainer instance;
        private PlayerInventory playerInventory;
        private GameObject player;
        private PlayerRobot playerRobot;
        private PlayerContainer() {
            player = GameObject.Find("Player");
            this.playerInventory = player.GetComponent<PlayerInventory>();
            this.playerRobot = player.GetComponent<PlayerRobot>();
        }
        public static PlayerContainer getInstance() {
            if (instance == null) {
                instance = new PlayerContainer();
            }
            return instance;
        }

        public PlayerInventory getInventory() {
            return playerInventory;
        }
        public Transform getTransform() {
            return player.transform;
        }
        public PlayerRobot getPlayerRobot() {
            return playerRobot;
        }
        public static void reset() {
            instance = null;
        }
    }
}


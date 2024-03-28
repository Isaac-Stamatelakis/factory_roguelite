using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PlayerModule {
    public class PlayerContainer
    {
        private static PlayerContainer instance;
        private PlayerInventory playerInventory;
        private PlayerContainer() {
            GameObject player = GameObject.Find("Player");
            this.playerInventory = player.GetComponent<PlayerInventory>();
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
    }
}


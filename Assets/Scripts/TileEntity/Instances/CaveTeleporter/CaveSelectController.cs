using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Caves;
using Dimensions;
using PlayerModule;

namespace TileEntityModule.Instances {
    public class CaveSelectController : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button teleportButton;
        private Cave currentCave;
        public void showCave(Cave cave) {
            teleportButton.onClick.RemoveAllListeners();
            teleportButton.onClick.AddListener(teleportButtonPress);
            teleportButton.gameObject.SetActive(true);
            currentCave = cave;
            nameText.text = cave.name;
            descriptionText.text = cave.Description;
        }

        public void showDefault() {
            nameText.text = "";
            descriptionText.text = "Click on a cave to view";
            teleportButton.gameObject.SetActive(false);
        }

        private void teleportButtonPress() {
            if (currentCave == null) {
                Debug.LogError("Tried to teleport to null cave");
                return;
            }
            Debug.Log("Teleporting to " + currentCave.name);
            Global.CurrentCave = currentCave; 
            CaveGenerator.generateCave();
            Transform playerTransform = PlayerContainer.getInstance().getTransform();
            DimensionManager.Instance.setPlayerSystem(playerTransform, -1,Vector2Int.zero);
        }
    }
}


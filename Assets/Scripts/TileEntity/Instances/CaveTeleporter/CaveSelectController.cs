using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Caves;
using Dimensions;

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

        private async void teleportButtonPress() {
            if (currentCave == null) {
                Debug.LogError("Tried to teleport to null cave");
                return;
            }
            Debug.Log("Teleporting to " + currentCave);
            Global.CurrentCave = currentCave; 
            CaveGenerator.generateCave();
            GameObject player = GameObject.Find("Player");
            player.transform.position = new Vector3(0,0,player.transform.position.z);
            await DimensionManager.Instance.setActiveSystemFromCellPosition(-1,Vector2Int.zero);
        }
    }
}


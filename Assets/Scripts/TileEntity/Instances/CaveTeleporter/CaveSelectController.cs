using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Generation;
using DimensionModule;

namespace TileEntityModule.Instances {
    public class CaveSelectController : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button teleportButton;
        private CaveRegion currentCave;
        public void showCave(CaveRegion caveRegion) {
            teleportButton.onClick.RemoveAllListeners();
            teleportButton.onClick.AddListener(teleportButtonPress);
            teleportButton.gameObject.SetActive(true);
            nameText.text = caveRegion.ToString();
            descriptionText.text = caveRegion.getDescription();
        }

        public void showDefault() {
            nameText.text = "";
            descriptionText.text = "Click on a cave to view";
            teleportButton.gameObject.SetActive(false);
        }

        private void teleportButtonPress() {
            GeneratedArea generatedArea = currentCave.getGeneratedArea();
            if (generatedArea == null) {
                Debug.LogError("CaveRegion did not have generated area");
                return;
            }
            Global.CurrentCave = generatedArea; 
            CaveGenerator.generateCave();
            GameObject player = GameObject.Find("Player");
            player.transform.position = new Vector3(0,0,player.transform.position.z);
            DimensionManagerContainer dimensionManagerContainer = DimensionManagerContainer.getInstance();
            dimensionManagerContainer.getManager().setDim(-1);
        }
    }
}

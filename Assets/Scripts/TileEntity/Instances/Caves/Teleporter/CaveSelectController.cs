using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Caves;
using Dimensions;
using PlayerModule;
using WorldModule;
using System.IO;
using Entities.Mobs;
using Player;
using TileEntity.Instances.Caves.Teleporter;
using UI;
using UI.Chat;
using UI.Statistics;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;
using World.Cave.TileDistributor;
using World.Cave.TileDistributor.Ore;
using World.Cave.TileDistributor.Standard;
using Debug = UnityEngine.Debug;

namespace TileEntity.Instances {
    public class CaveSelectController : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public Button teleportButton;
        public TextMeshProUGUI mNoSelectText;
        private CaveObject currentCaveObject;
        public CaveObject CurrentCaveObject => currentCaveObject;
        private CaveInstance caveInstance;
        
        [SerializeField] private CaveTeleporterParticles mParticlePrefab;
        public void ShowCave(CaveObject caveObject)
        {
            teleportButton.interactable = true;
            mNoSelectText.gameObject.SetActive(false);
            teleportButton.onClick.RemoveAllListeners();
            teleportButton.onClick.AddListener(() => {
                StartCoroutine(teleportButtonPress());
            });
            teleportButton.gameObject.SetActive(true);
            currentCaveObject = caveObject;
            nameText.text = caveObject.name;
            descriptionText.text = caveObject.Description;
        }

        public void DisplayEmpty()
        {
            mNoSelectText.gameObject.SetActive(true);
            nameText.text = "";
            descriptionText.text = "";
            teleportButton.interactable = false;
        }
        

        private IEnumerator teleportButtonPress() {
            if (!currentCaveObject) {
                Debug.LogError("Tried to teleport to null cave");
                yield break;
            }
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            CaveTeleporterParticles caveTeleporterParticles = Instantiate(mParticlePrefab, playerScript.transform, false);
            caveTeleporterParticles.transform.localPosition = Vector3.zero;
            Canvas parentCanvas = CanvasController.Instance.GetComponentInParent<Canvas>();
            parentCanvas.enabled = false;
            yield return StartCoroutine(caveTeleporterParticles.LoadParticles());
            yield return StartCoroutine(CaveUtils.LoadCave(currentCaveObject,CaveUtils.GenerateAndTeleportToCave));
            GameObject.Destroy(caveTeleporterParticles.gameObject);
            parentCanvas.enabled = true;
        }
    }
}


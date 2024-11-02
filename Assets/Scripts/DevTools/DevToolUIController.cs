using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using DevTools.Structures;
using UI.QuestBook;

namespace DevTools {
    public class DevToolUIController : MonoBehaviour
    {
        public UIAssetManager AssetManager;
        private static DevToolUIController instance;
        public static DevToolUIController Instance => instance;
        public void Awake() {
            instance = this;
        }
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject home;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button structureButton;
        [SerializeField] private Button questButton;
        private Transform currentUI;
        private string baseText;
        public void setTitleText(string text) {
            title.text = text;
        }
        public void setHomeVisibility(bool state) {
            home.SetActive(state);
            homeButton.gameObject.SetActive(!state);
        }
        public void addUI(Transform ui) {
            ui.transform.SetParent(transform,false);
            currentUI = ui;
        }
        public void resetTitleText() {
            title.text = baseText;
        }

        public void Start() {
            AssetManager.load();
            baseText = title.text;
            homeButton.onClick.AddListener(() => {
                setHomeVisibility(true);
                GameObject.Destroy(currentUI.gameObject);
                resetTitleText();
            });
            homeButton.gameObject.SetActive(false);

            structureButton.onClick.AddListener(() => {
                setHomeVisibility(false);
                StructureDevControllerUI structureDevControllerUI = AssetManager.cloneElement<StructureDevControllerUI>("STRUCTURE");
                structureDevControllerUI.init();
                addUI(structureDevControllerUI.transform);
                setTitleText("Structure Creator");
            });

            questButton.onClick.AddListener(() => {
                QuestBookCreationSceneController questBookCreationSceneController = AssetManager.cloneElement<QuestBookCreationSceneController>("QUEST");
            });
        }
    }
}


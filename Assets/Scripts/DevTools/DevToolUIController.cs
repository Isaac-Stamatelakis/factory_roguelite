using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using DevTools.Structures;
using DevTools.Upgrades;
using Items;
using UI.QuestBook;

namespace DevTools {
    public class DevToolUIController : CanvasController
    {
        private enum DevToolPage
        {
            Title,
            Structure,
            QuestBook,
            Upgrade
        }

        [SerializeField] private StructureDevControllerUI structureDevControllerUIPrefab;
        [SerializeField] private QuestBookCreationSceneController questBookCreationSceneControllerPrefab;
        [SerializeField] private DevToolUpgradeSelector upgradeSelectorUIPrefab;
        private static DevToolPage page = DevToolPage.Title;
        
        public override void EmptyListen()
        {
            
        }

        public override void ListenKeyPresses()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PopStack();
            }
        }

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject home;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button structureButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button robotToolButton;
        private Transform currentUI;
        private string baseText;
        public void SetTitleText(string text) {
            title.text = text;
        }
        public void SetHomeVisibility(bool state) {
            home.SetActive(state);
            homeButton.gameObject.SetActive(!state);
            PopStack();
        }
       
        public void ResetTitleText() {
            title.text = baseText;
        }

        public void Start() {
            homeButton.onClick.AddListener(() =>
            {
                page = DevToolPage.Title;
                Display();
            });
            
            structureButton.onClick.AddListener(() =>
            {
                page = DevToolPage.Structure;
                Display();
            });
            
            robotToolButton.onClick.AddListener(() =>
            {
                page = DevToolPage.Upgrade;
                Display();
            });
            questButton.onClick.AddListener(() => {
                QuestBookCreationSceneController questBookCreationSceneController = Instantiate(questBookCreationSceneControllerPrefab);
            });
            Display();
            StartCoroutine(ItemRegistry.LoadItems());
        }

        public void Display()
        {
            SetHomeVisibility(page == DevToolPage.Title);

            switch (page)
            {
                case DevToolPage.Title:
                case DevToolPage.QuestBook:
                    SetTitleText("Developer Tools");
                    break;
                case DevToolPage.Structure:
                    SetTitleText("Structure Generator");
                    StructureDevControllerUI structureDevControllerUI = GameObject.Instantiate(structureDevControllerUIPrefab);
                    structureDevControllerUI.Initialize();
                    DisplayObject(structureDevControllerUI.gameObject);
                    break;
                case DevToolPage.Upgrade:
                    SetTitleText("Robot Upgrades");
                    DevToolUpgradeSelector upgradeSelector = GameObject.Instantiate(upgradeSelectorUIPrefab);
                    upgradeSelector.Initialize();
                    DisplayObject(upgradeSelector.gameObject);
                    break;
                default:
                    
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}


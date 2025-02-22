using System;
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
        private enum DevToolPage
        {
            Title,
            Structure,
            QuestBook
        }

        [SerializeField] private StructureDevControllerUI structureDevControllerUIPrefab;
        [SerializeField] private QuestBookCreationSceneController questBookCreationSceneControllerPrefab;
        private static DevToolPage page = DevToolPage.Title;
       
        private static DevToolUIController instance;
        public static DevToolUIController Instance => instance;
        public void Awake() {
            instance = this;
            Display();
        }
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject home;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button structureButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button robotToolButton;
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
            homeButton.onClick.AddListener(() =>
            {
                page = DevToolPage.Title;
                Display();
                GameObject.Destroy(currentUI.gameObject);
            });
            
            structureButton.onClick.AddListener(() =>
            {
                page = DevToolPage.Structure;
                Display();
            });
            
            questButton.onClick.AddListener(() => {
                QuestBookCreationSceneController questBookCreationSceneController = Instantiate(questBookCreationSceneControllerPrefab);
            });
        }

        public void Display()
        {
            setHomeVisibility(page == DevToolPage.Title);

            switch (page)
            {
                case DevToolPage.Title:
                case DevToolPage.QuestBook:
                    setTitleText("Developer Tools");
                    break;
                case DevToolPage.Structure:
                    setTitleText("Structure Generator");
                    StructureDevControllerUI structureDevControllerUI = GameObject.Instantiate(structureDevControllerUIPrefab);
                    structureDevControllerUI.Initialize();
                    addUI(structureDevControllerUI.transform);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}


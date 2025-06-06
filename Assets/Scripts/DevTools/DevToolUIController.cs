using System;
using System.Collections;
using System.Collections.Generic;
using DevTools.CraftingTrees;
using DevTools.CraftingTrees.Selector;
using DevTools.ItemImageGenerator;
using DevTools.QuestBook;
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
            Upgrade,
            CraftingTree,
            ItemImageGen,
        }

        [SerializeField] private StructureDevControllerUI structureDevControllerUIPrefab;
        [SerializeField] private DevToolQuestBookListUI questBookListUIPrefab;
        [SerializeField] private DevToolUpgradeSelector upgradeSelectorUIPrefab;
        [SerializeField] private ItemImageGeneratorUI itemImageGeneratorPrefab;
        [SerializeField] private CraftingTreeSelectorUI craftingTreeSelectorUIPrefab;
        private static DevToolPage page = DevToolPage.Title;
        

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject home;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button structureButton;
        [SerializeField] private Button questButton;
        [SerializeField] private Button robotToolButton;
        [SerializeField] private Button itemImageGenButton;
        [SerializeField] private Button craftingTreeSelectorButton;
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

            void SwitchPage(DevToolPage newPage)
            {
                page = newPage;
                Display();
            }

            homeButton.onClick.AddListener(() =>
            {
                if (uiObjectStack.Count > 0)
                {
                    GameObject current = uiObjectStack.Peek().gameObject;
                    IDevToolBackButtonOverrideUI backButtonOverride = current.GetComponent<IDevToolBackButtonOverrideUI>();
                    if (backButtonOverride != null)
                    {
                        bool overriden = backButtonOverride.BackPressOverriden();
                        if (overriden) return;
                    }
                }
                SwitchPage(DevToolPage.Title);
            });
            
            structureButton.onClick.AddListener(() =>
            {
                SwitchPage(DevToolPage.Structure);
            });
            
            robotToolButton.onClick.AddListener(() =>
            {
                SwitchPage(DevToolPage.Upgrade);
            });
            questButton.onClick.AddListener(() =>
            {
                SwitchPage(DevToolPage.QuestBook);
            });
            itemImageGenButton.onClick.AddListener(() =>
            {
                SwitchPage(DevToolPage.ItemImageGen);
            });
            craftingTreeSelectorButton.onClick.AddListener(() =>
            {
                SwitchPage(DevToolPage.CraftingTree);
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
                    SetTitleText("Developer Tools");
                    break;
                case DevToolPage.QuestBook:
                    SetTitleText("Quest Book Libraries");
                    DevToolQuestBookListUI questBookListUI = GameObject.Instantiate(questBookListUIPrefab);
                    questBookListUI.Initialize();
                    DisplayObject(questBookListUI.gameObject);
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
                case DevToolPage.ItemImageGen:
                    SetTitleText("Item Image Generator");
                    ItemImageGeneratorUI itemImageGenerator = GameObject.Instantiate(itemImageGeneratorPrefab);
                    itemImageGenerator.Initialize();
                    DisplayObject(itemImageGenerator.gameObject);
                    break;
                case DevToolPage.CraftingTree:
                    SetTitleText("Crafting Trees");
                    CraftingTreeSelectorUI craftingTreeSelectorUI = GameObject.Instantiate(craftingTreeSelectorUIPrefab);
                    craftingTreeSelectorUI.Initialize();
                    DisplayObject(craftingTreeSelectorUI.gameObject);
                    break;
                default:
                    
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnInactiveEscapePress()
        {
            page = DevToolPage.Title;
            Display();
        }

        protected override void OnEscapePress()
        {
            if (uiObjectStack.Count <= 1)
            {
                page = DevToolPage.Title;
                Display();
                return;
            }
            PopStack();
        }
    }
}


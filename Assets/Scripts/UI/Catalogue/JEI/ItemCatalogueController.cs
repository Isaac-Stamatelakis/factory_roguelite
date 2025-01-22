using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using Items.Inventory;
using PlayerModule.KeyPress;


namespace UI.JEI
{
    enum CatalogueMode {
        Recipe,
        Cheat
    }
    public class ItemCatalogueController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button editButton;
        [SerializeField] private Image editButtonImage;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI pageDisplay;
        [SerializeField] private Transform resultContainer;
        public ItemSlotUI ItemSlotUIPrefab;
        private int page = 1;
        private int maxPages = 1;
        private static int COLUMNS = 6;
        private static int ROWS = 12;
        private int limit = ROWS*COLUMNS;
        private CatalogueMode mode = CatalogueMode.Recipe;
        private string lastSearch;
        private ItemHashTable displayed;
        private string currentSearch;
        private List<ItemSlot> queriedItems;
        
        internal CatalogueMode Mode { get => mode; set => mode = value; }
        
        void Start()
        {
            displayed = new ItemHashTable();
            
            searchField.onValueChanged.AddListener(OnSearchChange);
            
            editButton.onClick.AddListener(OnEditButtonClick);
            
            leftButton.onClick.AddListener(OnLeftButtonPress);
            
            rightButton.onClick.AddListener(OnRightButtonPress);
            
            PlayerKeyPressUtils.InitializeTypingListener(searchField);
        }

        public void ShowAll() {
            currentSearch = "";
            OnSearchChange("");
        }

        private void OnSearchChange(string search) {
            page = 1;
            queriedItems = ItemRegistry.GetInstance().QuerySlots(search,int.MaxValue);
            maxPages = Mathf.CeilToInt(queriedItems.Count/limit)+1;
            lastSearch = currentSearch;
            currentSearch = search;
            PopulateResults();
        }

        private void OnLeftButtonPress() {
            if (page > 1) {
                page -= 1;
            } else {
                page = maxPages;
            }
            PopulateResults();
        }

        private void OnRightButtonPress() {
            if (page < maxPages) {
                page += 1;
            } else {
                page = 1;
            }
            PopulateResults();
        }
        private void OnEditButtonClick()
        {
            if (!Input.GetKey(KeyCode.LeftControl)) return;
            switch (mode) {
                case CatalogueMode.Cheat:
                    mode = CatalogueMode.Recipe;
                    editButtonImage.color = Color.gray;
                    break;
                case CatalogueMode.Recipe:
                    mode = CatalogueMode.Cheat;
                    editButtonImage.color = Color.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateResults() {
            SetPageDisplay();
            List<ItemSlot> toDisplay;
            if (queriedItems.Count < page*limit) {
                toDisplay = queriedItems.GetRange((page-1)*limit,queriedItems.Count-(page-1)*limit);
            } else {
                toDisplay = queriedItems.GetRange((page-1)*limit,limit);
            }
            if (lastSearch == null || lastSearch.Length < currentSearch.Length) { // When appending to a search, order is always perserved
                ItemHashTable newDisplayHash = new ItemHashTable();
                foreach (ItemSlot itemSlot in toDisplay) {
                    newDisplayHash.addItem(itemSlot);
                }
                foreach (Transform previouslyDisplayed in resultContainer.transform) { 
                    ItemSlotUI itemSlotUI = previouslyDisplayed.GetComponent<ItemSlotUI>();
                    ItemSlot itemSlot = itemSlotUI.GetDisplayedSlot();
                    if (!newDisplayHash.containsItem(itemSlot)) {
                        GameObject.Destroy(previouslyDisplayed.gameObject);
                    }
                }   
                displayed = displayed.intersect(newDisplayHash);// Displayed objects is now only objects which were previously displayed and were still in query
                foreach (ItemSlot itemSlot in displayed.GetAllItems())
                {
                    itemSlot.amount = 1;
                }
                
            } else {  // When decreasing a search, order is not always perserved, so must clear.
                foreach (Transform previouslyDisplayed in resultContainer.transform) { 
                    GameObject.Destroy(previouslyDisplayed.gameObject);
                } 
                displayed = new ItemHashTable();
            } 

            foreach (ItemSlot itemSlot in toDisplay) {
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                
                if (displayed.getCount() >= limit) {
                    break;
                }
                if (displayed.containsItem(itemSlot)) { // Only create image if was not previously displayed
                    continue;
                }
                displayed.addItem(itemSlot);
                ItemSlotUI slotUI = Instantiate(ItemSlotUIPrefab, resultContainer);
                slotUI.Display(itemSlot);
                CatalogueElementClickHandler clickHandler = slotUI.gameObject.AddComponent<CatalogueElementClickHandler>();
                clickHandler.init(this,itemSlot);
            }
        }

        private void SetPageDisplay() {
            pageDisplay.text = page.ToString() + "/"  + maxPages.ToString();
        }


    }

}

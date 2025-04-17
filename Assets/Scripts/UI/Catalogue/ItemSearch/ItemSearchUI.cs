using System;
using System.Collections.Generic;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using PlayerModule.KeyPress;
using TMPro;
using UI.Catalogue.InfoViewer;
using UI.JEI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Catalogue.ItemSearch
{
    public class ItemSearchUI : MonoBehaviour
    {
        public VerticalLayoutGroup mVerticalLayoutGroup;
        public InventoryUI horizontalInventoryPrefab;
        enum CatalogueMode {
            Recipe,
            Cheat
        }
        
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button editButton;
        [SerializeField] private Image editButtonImage;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI pageDisplay;
        public InventoryUI[] horizontalInventories;
        private CatalogueMode mode = CatalogueMode.Recipe;
        private string lastSearch;
        private List<ItemSlot> queriedItems;
        private const int ROWS = 7;
        private const int COLUMNS = 15;
        private const int ITEMS_PER_PAGE = ROWS * COLUMNS;
        private int page = 1;
        private int maxPages = 1;
        private PlayerScript playerScript;
        private Action<ItemObject> onSelectOverride;
        private List<ItemObject> searchItems;

        public void Initialize(List<ItemObject> itemObjects, Action<ItemObject> onSelect)
        {
            this.searchItems = itemObjects;
            this.onSelectOverride = onSelect;
            Display();
        }

        public void Display()
        {
            GlobalHelper.DeleteAllChildren(mVerticalLayoutGroup.transform);
            searchField.onValueChanged.AddListener(OnSearchChange);
            
            editButton.onClick.AddListener(OnEditButtonClick);
            SetEditButtonColor();
            
            leftButton.onClick.AddListener(OnLeftButtonPress);
            rightButton.onClick.AddListener(OnRightButtonPress);
            
            CanvasController.Instance.AddTypingListener(searchField);
            
            horizontalInventories = new InventoryUI[ROWS];

            
            for (int row = 0; row < horizontalInventories.Length; row++)
            {
                InventoryUI inventory = Instantiate(horizontalInventoryPrefab, mVerticalLayoutGroup.transform);
                horizontalInventories[row] = inventory;
                inventory.InventoryInteractMode = InventoryInteractMode.OverrideAction;
                var row1 = row; // Keeps variable in scope for lambda

                void OnClick(PointerEventData.InputButton input, int index)
                {
                    OnItemClick(input, index, row1);
                }
                
                inventory.OverrideClickAction(OnClick);
            }

            OnSearchChange("");
        }
        public void Initialize(PlayerScript playerScript)
        {
            this.playerScript = playerScript;
            mode = playerScript.ItemSearchCheat ? CatalogueMode.Cheat : CatalogueMode.Recipe;
            Display();
        }

        void OnItemClick(PointerEventData.InputButton input, int col, int row)
        {
            ItemSlot itemSlot = horizontalInventories[row].GetItemSlot(col);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            if (onSelectOverride != null)
            {
                onSelectOverride.Invoke(itemSlot.itemObject);
                return;
            }
            switch (input)
            {
                case PointerEventData.InputButton.Left:
                    switch (mode) {
                        case CatalogueMode.Recipe:
                            CatalogueInfoUtils.DisplayItemInformation(itemSlot);
                            break;
                        case CatalogueMode.Cheat:
                            ItemSlot copy = ItemSlotFactory.Splice(itemSlot,Global.MAX_SIZE);
                            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                            grabbedItemProperties.SetItemSlot(copy);
                            break;
                    }
                    break;
                case PointerEventData.InputButton.Right:
                    switch (mode) {
                        case CatalogueMode.Recipe:
                            CatalogueInfoUtils.DisplayItemUses(itemSlot);
                            break;
                        case CatalogueMode.Cheat:
                            ItemSlot copy = ItemSlotFactory.Splice(itemSlot,1);
                            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                            if (grabbedItemProperties.ItemSlot == null || grabbedItemProperties.ItemSlot.itemObject == null) {
                                grabbedItemProperties.SetItemSlot(copy);
                            } else {
                                if (grabbedItemProperties.ItemSlot.itemObject.id == itemSlot.itemObject.id) {
                                    grabbedItemProperties.ItemSlot.amount = GlobalHelper.MinUInt(Global.MAX_SIZE, 1+grabbedItemProperties.ItemSlot.amount);
                                } else {
                                    grabbedItemProperties.SetItemSlot(itemSlot);
                                }
                            }
                            break;
                    }
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(input), input, null);
            }
        }
        private void OnSearchChange(string search) {
            page = 1;
            if (searchItems == null)
            {
                queriedItems = ItemRegistry.GetInstance().QuerySlots(search,int.MaxValue);
            }
            else
            {
                queriedItems = new List<ItemSlot>();
                foreach (ItemObject item in searchItems)
                {
                    if (!item.name.ToLower().Contains(search.ToLower())) continue;
                    queriedItems.Add(new ItemSlot(item,1,null));
                }
            }
            
            maxPages = Mathf.CeilToInt(queriedItems.Count/ITEMS_PER_PAGE)+1;
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
            playerScript.ItemSearchCheat = !playerScript.ItemSearchCheat;
            mode = playerScript.ItemSearchCheat ? CatalogueMode.Cheat : CatalogueMode.Recipe;
            SetEditButtonColor();
        }

        private void SetEditButtonColor()
        {
            switch (mode) {
                case CatalogueMode.Cheat:
                    editButtonImage.color = Color.red;
                    break;
                case CatalogueMode.Recipe:
                    editButtonImage.color = Color.gray;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PopulateResults() {
            SetPageDisplay();
            List<ItemSlot> toDisplay;
            if (queriedItems.Count < page*ITEMS_PER_PAGE) {
                toDisplay = queriedItems.GetRange((page-1)*ITEMS_PER_PAGE,queriedItems.Count-(page-1)*ITEMS_PER_PAGE);
            } else {
                toDisplay = queriedItems.GetRange((page-1)*ITEMS_PER_PAGE,ITEMS_PER_PAGE);
            }
            
            int row = 0;
            while (row < ROWS)
            {
                List<ItemSlot> rowSlots = new List<ItemSlot>();
                int col = 0;
                while (col < COLUMNS)
                {
                    int idx = row * COLUMNS + col;
                    if (idx >= toDisplay.Count)
                    {
                        rowSlots.Add(null);
                    }
                    else
                    {
                        rowSlots.Add(toDisplay[idx]);
                    }
                   
                    col++;
                }
                horizontalInventories[row].DisplayInventory(rowSlots);
                row++;
            }
        }

        private void SetPageDisplay() {
            pageDisplay.text = page.ToString() + "/"  + maxPages.ToString();
        }
    }
}

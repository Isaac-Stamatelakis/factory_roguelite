using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Items;
using Items.Inventory;


namespace UI.JEI
{
    enum CatalogueMode {
        Recipe,
        Cheat
    }
    public class ItemCatalogueController : MonoBehaviour
    {
        private int page = 1;
        private int maxPages = 1;
        private static int COLUMNS = 6;
        private static int ROWS = 12;
        private int limit = ROWS*COLUMNS;
        private CatalogueMode mode = CatalogueMode.Recipe;
        private string lastSearch;
        private ItemHashTable displayed;
        private Transform resultContainer;
        private Image editButtonImage;
        private string currentSearch;
        private List<ItemSlot> queriedItems;
        private TextMeshProUGUI pageDisplay;

        internal CatalogueMode Mode { get => mode; set => mode = value; }

        // Start is called before the first frame update
        void Start()
        {
            displayed = new ItemHashTable();
            GameObject searchContainer = Global.findChild(transform,"SearchContainer");
            GameObject searchObject = Global.findChild(searchContainer.transform,"Search");

            TMP_InputField search = searchObject.GetComponent<TMP_InputField>();
            search.onValueChanged.AddListener(onSearchChange);

            GameObject editButton = Global.findChild(searchContainer.transform, "EditButton");
            editButton.GetComponent<Button>().onClick.AddListener(onEditButtonClick);
            editButtonImage = editButton.GetComponent<Image>();

            GameObject leftButton = Global.findChild(transform,"LeftButton");
            leftButton.GetComponent<Button>().onClick.AddListener(onLeftButtonPress);

            GameObject rightButton = Global.findChild(transform,"RightButton");
            rightButton.GetComponent<Button>().onClick.AddListener(onRightButtonPress);

            pageDisplay = Global.findChild(transform,"PageCounter").GetComponent<TextMeshProUGUI>();
            resultContainer = Global.findChild(transform,"Results").transform;
        }

        public void showAll() {
            currentSearch = "";
            onSearchChange("");
        }

        private void onSearchChange(string search) {
            page = 1;
            queriedItems = ItemRegistry.getInstance().querySlots(search,int.MaxValue);
            maxPages = Mathf.CeilToInt(queriedItems.Count/limit)+1;
            lastSearch = currentSearch;
            currentSearch = search;
            populateResults();
        }

        private void onLeftButtonPress() {
            if (page > 1) {
                page -= 1;
            } else {
                page = maxPages;
            }
            populateResults();
        }

        private void onRightButtonPress() {
            if (page < maxPages) {
                page += 1;
            } else {
                page = 1;
            }
            populateResults();
        }
        private void onEditButtonClick() {
            if (Input.GetKey(KeyCode.LeftControl)) {
                switch (mode) {
                    case CatalogueMode.Cheat:
                        mode = CatalogueMode.Recipe;
                        editButtonImage.color = Color.gray;
                        break;
                    case CatalogueMode.Recipe:
                        mode = CatalogueMode.Cheat;
                        editButtonImage.color = Color.red;
                        break;
                }
            }
        }

        private void populateResults() {
            setPageDisplay();
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
            } else {  // When decreasing a search, order is not always perserved, so must clear.
                foreach (Transform previouslyDisplayed in resultContainer.transform) { 
                    GameObject.Destroy(previouslyDisplayed.gameObject);
                } 
                displayed = new ItemHashTable();
            } 

            foreach (ItemSlot itemSlot in toDisplay) {
                if (displayed.getCount() >= limit) {
                    break;
                }
                if (displayed.containsItem(itemSlot)) { // Only create image if was not previously displayed
                    continue;
                }
                displayed.addItem(itemSlot);
                ItemSlotUI slotUI = ItemSlotUIFactory.newItemSlotUI(itemSlot,resultContainer,null,false);
                CatalogueElementClickHandler clickHandler = slotUI.gameObject.AddComponent<CatalogueElementClickHandler>();
                clickHandler.init(this,itemSlot);
            }
        }

        private void setPageDisplay() {
            pageDisplay.text = page.ToString() + "/"  + maxPages.ToString();
        }


    }

}

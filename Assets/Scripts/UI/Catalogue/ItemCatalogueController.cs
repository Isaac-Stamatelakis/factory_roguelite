using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ItemModule;
using ItemModule.Inventory;


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
        private int limit = 6*12;
        private CatalogueMode mode = CatalogueMode.Recipe;
        private string lastSearch;
        private HashSet<string> displayedIDs;
        private Transform resultContainer;
        private Image editButtonImage;
        private string currentSearch;
        private List<ItemObject> queriedItems;
        private TextMeshProUGUI pageDisplay;

        internal CatalogueMode Mode { get => mode; set => mode = value; }

        // Start is called before the first frame update
        void Start()
        {
            displayedIDs = new HashSet<string>();
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
            onSearchChange("");
            
        }

        private void onSearchChange(string search) {
            page = 1;
            queriedItems = ItemRegistry.getInstance().query(search,int.MaxValue);
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
            List<ItemObject> toDisplay;
            if (queriedItems.Count < page*limit) {
                toDisplay = queriedItems.GetRange((page-1)*limit,queriedItems.Count-(page-1)*limit);
            } else {
                toDisplay = queriedItems.GetRange((page-1)*limit,limit);
            }
            if (lastSearch == null || lastSearch.Length < currentSearch.Length) { // When appending to a search, order is always perserved
                HashSet<string> newDisplay = new HashSet<string>();
                foreach (ItemObject itemObject1 in toDisplay) {
                    newDisplay.Add(itemObject1.id);
                }
                foreach (Transform previouslyDisplayed in resultContainer.transform) { 
                    if (!newDisplay.Contains(previouslyDisplayed.name)) { // name is id
                        GameObject.Destroy(previouslyDisplayed.gameObject);
                    }
                }   
                displayedIDs.IntersectWith(newDisplay); // Displayed objects is now only objects which were previously displayed and were still in query
            } else {  // When decreasing a search, order is not always perserved, so must clear.
                foreach (Transform previouslyDisplayed in resultContainer.transform) { 
                    GameObject.Destroy(previouslyDisplayed.gameObject);
                } 
                displayedIDs.Clear();
            } 

            foreach (ItemObject itemObject in toDisplay) {
                if (displayedIDs.Count >= limit) {
                    break;
                }
                if (!displayedIDs.Contains(itemObject.id)) { // Only create image if was not previously displayed
                    displayedIDs.Add(itemObject.id);
                    GameObject itemObjectImage = new GameObject();
                    CatalogueElementClickHandler clickHandler = itemObjectImage.AddComponent<CatalogueElementClickHandler>();
                    clickHandler.init(this,itemObject);
                    itemObjectImage.transform.SetParent(resultContainer);
                    itemObjectImage.name = itemObject.id;
                    Image image = itemObjectImage.AddComponent<Image>();
                    image.sprite = itemObject.getSprite();
                    Vector2 size = ItemSlotUIFactory.getItemSize(image.sprite);
                    if (size.x > size.y) {
                        itemObjectImage.transform.localScale = new Vector3(1,size.y/size.x,1);
                    } else if (size.y > size.x) {
                        itemObjectImage.transform.localScale = new Vector3(size.x/size.y,1,1);
                    } else {
                        itemObjectImage.transform.localScale = new Vector3(size.x/64,size.y/64,1);
                    }
                }
            }
        }

        private void setPageDisplay() {
            pageDisplay.text = page.ToString() + "/"  + maxPages.ToString();
        }


    }

}

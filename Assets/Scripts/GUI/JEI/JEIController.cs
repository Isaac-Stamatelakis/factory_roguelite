using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class JEIController : MonoBehaviour
{
    private string lastSearch;
    private int limit = 100;
    private HashSet<string> displayedIDs;
    private Transform resultContainer;

    // Start is called before the first frame update
    void Start()
    {
        displayedIDs = new HashSet<string>();
        GameObject searchContainer = Global.findChild(transform,"Search");
        TMP_InputField search = searchContainer.GetComponent<TMP_InputField>();
        search.onValueChanged.AddListener(onSearchChange);
        resultContainer = Global.findChild(transform,"Results").transform;
        onSearchChange("");
        
    }

    private void onSearchChange(string search) {
        populateResults(ItemRegister.getInstance().query(search,limit),search);
    }

    private void populateResults(List<ItemObject> toDisplay, string search) {
        if (lastSearch == null || lastSearch.Length < search.Length) { // When appending to a search, order is always perserved
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
            if (!displayedIDs.Contains(itemObject.id)) { // Only create image if was not previously displayed
                if (itemObject is TransmutableItemObject) {
                    continue; // TODO Implement TransmutableItems for JEI (Mutable will be displayed for each itemobject)
                }
                displayedIDs.Add(itemObject.id);
                GameObject itemObjectImage = new GameObject();
                itemObjectImage.transform.SetParent(resultContainer);
                itemObjectImage.name = itemObject.id;
                Image image = itemObjectImage.AddComponent<Image>();
                image.sprite = itemObject.sprite;
                Vector2 size = InventoryGrid.getItemSize(itemObject.sprite);
                if (size.x > size.y) {
                    itemObjectImage.transform.localScale = new Vector3(1,size.y/size.x,1);
                } else if (size.y > size.x) {
                    itemObjectImage.transform.localScale = new Vector3(size.x/size.y,1,1);
                } else {
                    itemObjectImage.transform.localScale = new Vector3(size.x/64,size.y/64,1);
                }
            }
        }
        lastSearch = search;
    }
}

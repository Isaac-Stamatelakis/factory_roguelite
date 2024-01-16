using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
public class PlayerInventory : MonoBehaviour
{

    private static int entityLayer;
    private int selectedSlot = 0;
    private static Vector2Int inventorySize = new Vector2Int(10,4);
    private List<Dictionary<string,object>> inventory;
    private PlayerInventoryGrid playerInventoryGrid;
    private GameObject inventoryContainer;
    private GameObject inventoryItemContainer;
    private GameObject uiPlayerInventoryContainer;
    private GameObject hotbarNumbersContainer;
    private bool expanded = false;
    // Start is called before the first frame update
    void Start()
    {
            entityLayer = 1 << LayerMask.NameToLayer("Entity");
            initInventory();
    }

    private void initInventory() {
        GetComponent<PlayerIO>().initRead();
        GameObject canvas = GameObject.Find("UICanvas");
        uiPlayerInventoryContainer = Global.findChild(canvas.transform, "PlayerInventory");
        loadInventoryUI();

    }

    private void loadInventoryUI() {
        inventory = GetComponent<PlayerIO>().getPlayerInventory();
        hotbarNumbersContainer = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/GUI/InventoryHotbar"));
        GameObject inventoryContainer = Global.findChild(hotbarNumbersContainer.transform,"Inventory");
        hotbarNumbersContainer.transform.SetParent(uiPlayerInventoryContainer.transform,false);
        playerInventoryGrid = inventoryContainer.AddComponent<PlayerInventoryGrid>();
        playerInventoryGrid.initalize(inventory,new Vector2Int(10,1));
    }
    // Update is called 50 times per second
    void Update()
    {
       raycastHitTileEntities();
    }

    public void toggleInventory() {
        expanded = !expanded;
        if (expanded) {
            playerInventoryGrid.updateSize(new Vector2Int(10,4));
        } else {
            playerInventoryGrid.updateSize(new Vector2Int(10,1));
        }
    }
    public void changeSelectedSlot(int slot) {
        selectedSlot = slot;
        playerInventoryGrid.selectSlot(slot);
        
    }
    
    private void raycastHitTileEntities() {
        Vector2 position = new Vector2(transform.position.x-0.25f,transform.position.y);
        RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.5f,Vector2.zero, 0.25f, entityLayer);
        foreach (RaycastHit2D hit in hits) {
            ItemEntityProperties itemEntityProperities = hit.collider.gameObject.GetComponent<ItemEntityProperties>();
            
            if (itemEntityProperities != null) {
                if (itemEntityProperities.LifeTime < 1f) {
                    continue;
                }
                bool alreadyInInventory = false;
                int firstOpenSlot = -1;
                for (int n = inventory.Count-1; n >= 0; n --) {
                    Dictionary<string,object> inventorySlot = inventory[n];
                    if (inventorySlot == null || Convert.ToInt32(inventorySlot["id"]) == -1) {
                        firstOpenSlot = n;
                        continue;
                    }
                    int id = Convert.ToInt32(inventorySlot["id"]);
                    int amount = Convert.ToInt32(inventorySlot["amount"]);
                    if (id == itemEntityProperities.Id && amount < Global.MaxSize) {
                        alreadyInInventory = true;
                        amount += itemEntityProperities.Amount;
                        itemEntityProperities.Amount = amount;
                        if (amount > Global.MaxSize) {
                            amount = Global.MaxSize; 
                        }
                        
                        itemEntityProperities.Amount -= amount;
                        if (itemEntityProperities.Amount <= 0) {
                            Destroy(itemEntityProperities.gameObject);
                        }
                        playerInventoryGrid.updateAmount(n,amount);
                    }
                }
                if (!alreadyInInventory && firstOpenSlot >= 0) {
                    inventory[firstOpenSlot] = new Dictionary<string, object>{
                            {"id", itemEntityProperities.Id},
                            {"amount",itemEntityProperities.Amount}
                    };
                    Destroy(itemEntityProperities.gameObject);
                    if (firstOpenSlot < inventorySize.x * inventorySize.y) {
                        playerInventoryGrid.setItem(firstOpenSlot, inventory[firstOpenSlot]);
                    }
                }
            }
        }
    }
    

    public void deiterateInventoryAmount() {
        Dictionary<string,object> itemInventoryData = inventory[selectedSlot];
        if (itemInventoryData == null) {
            return;
        }
        int amount = Convert.ToInt32(itemInventoryData["amount"]);
        amount--;
        if (amount == 0) {
            itemInventoryData["id"] = -1;
            playerInventoryGrid.unloadItem(selectedSlot);
        } else {
            playerInventoryGrid.updateAmount(selectedSlot,amount);
        }
    }

    public void iterateSelectedTile(int iterator) {
        selectedSlot += iterator;
        selectedSlot = (int) Global.modInt(selectedSlot,playerInventoryGrid.Size.x);
        changeSelectedSlot(selectedSlot);

        
    }

    public int getSelectedTileId() {
        if (inventory[selectedSlot] == null) {
            return -1;
        }
        return Convert.ToInt32(inventory[selectedSlot]["id"]);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using PlayerModule.IO;
using ItemModule.Inventory;
using ItemModule;
using DimensionModule;
using ChunkModule;

namespace PlayerModule {

    public class PlayerInventory : MonoBehaviour, IInventoryListener
    {

        private static int entityLayer;
        private int selectedSlot = 0;
        private static UnityEngine.Vector2Int inventorySize = new UnityEngine.Vector2Int(10,4);
        private List<ItemSlot> inventory;
        private PlayerInventoryGrid playerInventoryGrid;
        private GameObject inventoryContainer;
        private GameObject inventoryItemContainer;
        private GameObject uiPlayerInventoryContainer;
        private GameObject hotbarNumbersContainer;
        private bool expanded = false;

        public List<ItemSlot> Inventory { get => inventory; set => inventory = value; }

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
            inventory = ItemSlotFactory.deserialize(GetComponent<PlayerIO>().getPlayerInventoryData());
            playerInventoryGrid = uiPlayerInventoryContainer.AddComponent<PlayerInventoryGrid>();
            playerInventoryGrid.initalize(inventory, new UnityEngine.Vector2Int(10,1));
        }

        public void cloneInventoryUI(Transform container) {
            PlayerInventoryGrid cloneGrid = container.gameObject.AddComponent<PlayerInventoryGrid>();
            cloneGrid.initalize(inventory,new Vector2Int(10,4));
        }
        // Update is called 50 times per second
        void Update()
        {
            raycastHitTileEntities();
        }

        public void toggleInventory() {
            expanded = !expanded;
            if (expanded) {
                playerInventoryGrid.updateSize(new UnityEngine.Vector2Int(10,4));
            } else {
                playerInventoryGrid.updateSize(new UnityEngine.Vector2Int(10,1));
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
                ItemEntity itemEntityProperities = hit.collider.gameObject.GetComponent<ItemEntity>();
                
                if (itemEntityProperities != null) {
                    if (itemEntityProperities.LifeTime < 1f) {
                        continue;
                    }
                    bool alreadyInInventory = false;
                    int firstOpenSlot = -1;
                    for (int n = inventory.Count-1; n >= 0; n --) {
                        ItemSlot inventorySlot = inventory[n];
                        if (inventorySlot == null || inventorySlot.itemObject == null) {
                            firstOpenSlot = n;
                            continue;
                        }
                        if (inventorySlot.itemObject.id == itemEntityProperities.itemSlot.itemObject.id && inventorySlot.amount < Global.MaxSize) {
                            alreadyInInventory = true;
                            inventorySlot.amount += itemEntityProperities.itemSlot.amount;
                            itemEntityProperities.itemSlot.amount = inventorySlot.amount;
                            if (inventorySlot.amount > Global.MaxSize) {
                                inventorySlot.amount = Global.MaxSize; 
                            }
                            
                            itemEntityProperities.itemSlot.amount -= inventorySlot.amount;
                            if (itemEntityProperities.itemSlot.amount <= 0) {
                                Destroy(itemEntityProperities.gameObject);
                            }
                            playerInventoryGrid.reloadItem(n);
                        }
                    }
                    if (!alreadyInInventory && firstOpenSlot >= 0) {
                        inventory[firstOpenSlot] = itemEntityProperities.itemSlot;
                        Destroy(itemEntityProperities.gameObject);
                        if (firstOpenSlot < inventorySize.x * inventorySize.y) {
                            playerInventoryGrid.setItem(firstOpenSlot, inventory[firstOpenSlot]);
                        }
                    }
                }
            }
        }
        

        public void deiterateInventoryAmount() {
            ItemSlot itemInventoryData = inventory[selectedSlot];
            if (itemInventoryData == null) {
                return;
            }
            inventory[selectedSlot].amount--;
            if (inventory[selectedSlot].amount == 0) {
                inventory[selectedSlot] = null;
                
                playerInventoryGrid.unloadItem(selectedSlot);
            } else {
                playerInventoryGrid.reloadItem(selectedSlot);
            }
        }

        public void iterateSelectedTile(int iterator) {
            selectedSlot += iterator;
            selectedSlot = (int) Global.modInt(selectedSlot,playerInventoryGrid.Size.x);
            changeSelectedSlot(selectedSlot); 
        }

        public void give(ItemSlot itemSlot) {
            if (!ItemSlotHelper.canInsertIntoInventory(inventory,itemSlot,Global.MaxSize)) {
                DimensionManager dimensionManager = DimensionManagerContainer.getManager();
                IChunk chunk = dimensionManager.ActiveSystem.getChunk(Global.getCellPositionFromWorld(transform.position));
                if (chunk is not ILoadedChunk loadedChunk) {
                    return;
                }
                ItemEntityHelper.spawnItemEntity(transform.position,itemSlot,loadedChunk.getEntityContainer());
            } else {
                ItemSlotHelper.insertIntoInventory(inventory,itemSlot,Global.MaxSize);
            }
        }

        public string getSelectedId() {
            if (inventory[selectedSlot] == null || inventory[selectedSlot].itemObject == null) {
                return null;
            }
            return inventory[selectedSlot].itemObject.id;
        }

        public ItemSlot getSelectedItemSlot() {
            return inventory[selectedSlot];
        }

        public void removeSelectedItemSlot() {
            inventory[selectedSlot] = null;
        }

        public string getJson() {
            return ItemSlotFactory.serializeList(inventory);
        }

        public void inventoryUpdate(int n)
        {
            
        }

        public void hideUI() {
            playerInventoryGrid.gameObject.SetActive(false);
        }

        public void showUI() {
            playerInventoryGrid.gameObject.SetActive(true);
        }
    }

    public interface IPlayerInventoryIntegratedUI {
        
    }
}
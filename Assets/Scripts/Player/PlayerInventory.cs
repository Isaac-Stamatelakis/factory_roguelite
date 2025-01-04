using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using PlayerModule.IO;
using Items.Inventory;
using Items;
using Dimensions;
using Chunks;
using UnityEngine.UI;
using Entities;
using Item.Slot;

namespace PlayerModule {
    public class PlayerInventory : MonoBehaviour, IInventoryListener
    {
        public static readonly int COLUMNS = 10;
        [SerializeField] private PlayerRobot playerRobot;
        [SerializeField] private InventoryUI playerInventoryGrid;
        //private InventoryDisplayMode mode = InventoryDisplayMode.Inventory;
        private static int entityLayer;
        private int selectedSlot = 0;
        private static UnityEngine.Vector2Int inventorySize = new UnityEngine.Vector2Int(10,4);
        private List<ItemSlot> inventory;
        private List<ItemSlot> toolInventory;
        private GameObject inventoryContainer;
        private GameObject inventoryItemContainer;
        private GameObject hotbarNumbersContainer;
        private bool expanded = false;

        public List<ItemSlot> Inventory { get => inventory; set => inventory = value; }

        // Start is called before the first frame update
        void Start()
        {
            entityLayer = 1 << LayerMask.NameToLayer("Entity");
        }

        public void Initalize() {
            GetComponent<PlayerIO>().initRead();
            inventory = ItemSlotFactory.Deserialize(GetComponent<PlayerIO>().getPlayerInventoryData());
            if (inventory == null)
            {
                Debug.LogError("Error deserializing player inventory");
                inventory = ItemSlotFactory.createEmptyInventory(40);
            }
            playerInventoryGrid.DisplayInventory(inventory,10);
        }
        
        
        void Update()
        {
            raycastHitTileEntities();
        }

        public void toggleInventory() {
            expanded = !expanded;
            if (expanded) {
                playerInventoryGrid.DisplayInventory(inventory);
            } else {
                playerInventoryGrid.DisplayInventory(inventory,10);
            }
        }
        public void ChangeSelectedSlot(int slot) {
            selectedSlot = slot;
            playerInventoryGrid.HighlightSlot(slot);
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
                            playerInventoryGrid.DisplayItem(n);
                        }
                    }
                    if (!alreadyInInventory && firstOpenSlot >= 0) {
                        inventory[firstOpenSlot] = itemEntityProperities.itemSlot;
                        Destroy(itemEntityProperities.gameObject);
                        if (firstOpenSlot < inventorySize.x * inventorySize.y) {
                            playerInventoryGrid.SetItem(firstOpenSlot, inventory[firstOpenSlot]);
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
            }
            playerInventoryGrid.DisplayItem(selectedSlot);
        }

        public void iterateSelectedTile(int iterator) {
            selectedSlot += iterator;
            selectedSlot = (int) Global.modInt(selectedSlot,COLUMNS);
            ChangeSelectedSlot(selectedSlot); 
        }

        public void give(ItemSlot itemSlot) {
            if (!ItemSlotUtils.CanInsertIntoInventory(inventory,itemSlot,Global.MaxSize)) {
                IChunk chunk = DimensionManager.Instance.getPlayerSystem(transform).getChunk(Global.getCellPositionFromWorld(transform.position));
                if (chunk is not ILoadedChunk loadedChunk) {
                    return;
                }
                ItemEntityHelper.spawnItemEntity(transform.position,itemSlot,loadedChunk.getEntityContainer());
            } else {
                ItemSlotUtils.InsertIntoInventory(inventory,itemSlot,Global.MaxSize);
            }
        }

        public string getSelectedId() {
            if (inventory == null) {
                return null;
            }
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

        public void InventoryUpdate(int n)
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

    public enum InventoryDisplayMode {
            Inventory,
            Tools
        }
}
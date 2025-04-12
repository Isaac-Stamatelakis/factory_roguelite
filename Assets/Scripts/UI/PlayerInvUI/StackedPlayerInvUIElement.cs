using System;
using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using Items.Inventory;
using PlayerModule;
using TileEntity.Instances.Machine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInvUI
{
    public class StackedPlayerInvUIElement : MonoBehaviour
    {
        [SerializeField] private InventoryUI playerInventoryUI;
        [SerializeField] private InventoryUI trashCanUI;
        [SerializeField] private GameObject playerInventoryContainer;
        [SerializeField] private Button takeAllButton;
        [SerializeField] private Button giveAllButton;
        [SerializeField] private Button quickStackButton;

        private InventoryUI originalPlayerInventoryUI;
        public void Start()
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            playerInventoryUI.DisplayInventory(playerInventory.Inventory);
            originalPlayerInventoryUI = playerInventory.InventoryUI;
            trashCanUI.DisplayInventory(new List<ItemSlot>{null},false);
            playerInventoryUI.AddCallback(RefreshBaseUI);
        }

        public void Update()
        {
            trashCanUI.SetItem(0,null);
        }

        private void RefreshBaseUI(int index)
        {
            originalPlayerInventoryUI.RefreshSlots();
        }

        public void DisplayWithPlayerInventory(GameObject uiObject, bool below)
        {
            IInventoryUITileEntityUI inventoryUITileEntityUI = GetTileEntityUI(uiObject);
            SyncTileEntityUI(inventoryUITileEntityUI);
            
            uiObject.transform.SetParent(transform,false);
            if (!below)
            {
                uiObject.transform.SetAsFirstSibling();
            }
        }
        
        private IInventoryUITileEntityUI GetTileEntityUI(GameObject uiObject)
        {
            IInventoryUITileEntityUI tileEntityUI = uiObject.GetComponent<IInventoryUITileEntityUI>();
            if (tileEntityUI != null) return tileEntityUI;
            
            IInventoryUIAggregator aggregator = uiObject.GetComponent<IInventoryUIAggregator>();
            return aggregator?.GetUITileEntityUI();
        }

        private void SyncTileEntityUI(IInventoryUITileEntityUI inventoryUITileEntity)
        {
            bool inventoryConnected = inventoryUITileEntity != null;
            giveAllButton.gameObject.SetActive(inventoryConnected);
            giveAllButton.gameObject.SetActive(inventoryConnected);
            if (!inventoryConnected) return;
           
            takeAllButton.onClick.AddListener(() =>
            {
                OnInventoryInteractPress(InventoryInteractType.Take);
            });
            giveAllButton.onClick.AddListener(() =>
            {
                OnInventoryInteractPress(InventoryInteractType.Give);
            });
            playerInventoryUI.SetConnection(inventoryUITileEntity.GetInput());
            inventoryUITileEntity.GetInput().SetConnection(playerInventoryUI);
            List<InventoryUI> inventories = inventoryUITileEntity.GetAllInventoryUIs();
            
            if (inventories == null) return;
            foreach (InventoryUI inventoryUI in inventories)
            {
                inventoryUI?.SetConnection(playerInventoryUI);
            }
            return;
            
            void OnInventoryInteractPress(InventoryInteractType inventoryGiveMode)
            {
                foreach (InventoryUI inventoryUI in inventoryUITileEntity.GetAllInventoryUIs())
                {
                    List<ItemSlot> itemSlots = inventoryUI.GetInventory();
                    if (itemSlots == null || itemSlots.Count == 0) continue;
                    ItemState itemState = inventoryUI.GetItemSlotUI(0).ItemState;
                    if (itemState != ItemState.Solid) continue; // Only need check the first item slot ui
                    switch (inventoryGiveMode)
                    {
                        case InventoryInteractType.Take:
                            ItemSlotUtils.InsertInventoryIntoInventory(playerInventoryUI.GetInventory(),itemSlots,Global.MAX_SIZE);
                            break;
                        case InventoryInteractType.Give:
                            ItemSlotUtils.InsertInventoryIntoInventory(itemSlots,playerInventoryUI.GetInventory(),Global.MAX_SIZE);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(inventoryGiveMode), inventoryGiveMode, null);
                    }
                    inventoryUI.RefreshSlots();
                }
                playerInventoryUI.RefreshSlots();
                originalPlayerInventoryUI.RefreshSlots();
            }
        }
        
        private enum InventoryInteractType
        {
            Take,
            Give,
        }
        
        public void SetBackgroundColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        
    }
}

using System;
using System.Collections.Generic;
using Item.Slot;
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
            if (inventoryUITileEntity == null) return;
            
            playerInventoryUI.SetConnection(inventoryUITileEntity.GetInput());
            inventoryUITileEntity.GetInput().SetConnection(playerInventoryUI);
            List<InventoryUI> inventories = inventoryUITileEntity.GetAllInventoryUIs();
            
            if (inventories == null) return;
            foreach (InventoryUI inventoryUI in inventories)
            {
                inventoryUI?.SetConnection(playerInventoryUI);
            }
        }

        public void SetBackgroundColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        
    }
}

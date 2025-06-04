using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Systems;
using Conduits.Ports;
using Item.Display;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using Player.UI.Inventory;
using PlayerModule;
using TileEntity;
using TileEntity.Instances.Machine.UI;
using UI.ToolTip;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInvUI
{
    public class StackedPlayerInvUIElement : MonoBehaviour
    {
        [SerializeField] private InventoryUI playerInventoryUI;
        [SerializeField] private GameObject playerInventoryContainer;
        [SerializeField] private InventoryUtilUI inventoryUtilUI;
        private InventoryUI originalPlayerInventoryUI;
        public InventoryUI PlayerInventoryUI => playerInventoryUI;
        public void Start()
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            playerInventoryUI.DisplayInventory(playerInventory.Inventory);
            originalPlayerInventoryUI = playerInventory.InventoryUI;
            playerInventoryUI.AddCallback(RefreshBaseUI);
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
            inventoryUtilUI.SyncTileEntityUI(PlayerManager.Instance.GetPlayer(), playerInventoryUI, inventoryUITileEntity);
            bool inventoryConnected = inventoryUITileEntity != null;

            if (!inventoryConnected) return;

            playerInventoryUI.SetConnection(inventoryUITileEntity.GetInput());
            inventoryUITileEntity.GetInput().SetConnection(playerInventoryUI);
            List<InventoryUI> inventories = inventoryUITileEntity.GetAllInventoryUIs();

            if (inventories == null) return;
            foreach (InventoryUI inventoryUI in inventories)
            {
                inventoryUI?.SetConnection(playerInventoryUI);
            }
        }

        
    }
}

using System;
using System.Collections.Generic;
using Chunks;
using Chunks.Systems;
using Conduits.Ports;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using Items.Inventory;
using Player;
using PlayerModule;
using TileEntity;
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
            quickStackButton.gameObject.SetActive(inventoryConnected);
            if (!inventoryConnected) return;

            takeAllButton.onClick.AddListener(TakeAllPress);
            giveAllButton.onClick.AddListener(GiveAllPress);
            quickStackButton.onClick.AddListener(OnQuickStackPress);
            
            playerInventoryUI.SetConnection(inventoryUITileEntity.GetInput());
            inventoryUITileEntity.GetInput().SetConnection(playerInventoryUI);
            List<InventoryUI> inventories = inventoryUITileEntity.GetAllInventoryUIs();
            
            if (inventories == null) return;
            foreach (InventoryUI inventoryUI in inventories)
            {
                inventoryUI?.SetConnection(playerInventoryUI);
            }
            return;

            void OnQuickStackPress()
            {
                InventoryUI inputInventoryUI = inventoryUITileEntity.GetInput();
                if (!inputInventoryUI || !CanInsertIntoInventory(inputInventoryUI)) return;
                ItemSlotUtils.QuickStackInventoryIntoInventory(inputInventoryUI.GetInventory(),playerInventoryUI.GetInventory(),Global.MAX_SIZE);
                inputInventoryUI.RefreshSlots();
                RefreshPlayerInventoryUI();
            }

            void GiveAllPress()
            {
                InventoryUI inputInventoryUI = inventoryUITileEntity.GetInput();
                if (!inputInventoryUI || !CanInsertIntoInventory(inputInventoryUI)) return;
                ItemSlotUtils.InsertInventoryIntoInventory(inputInventoryUI.GetInventory(),playerInventoryUI.GetInventory(),Global.MAX_SIZE);
                inputInventoryUI.RefreshSlots();
                RefreshPlayerInventoryUI();
            }

            void TakeAllPress()
            {
                InventoryUI inputInventoryUI = inventoryUITileEntity.GetInput();
                List<InventoryUI> connections = inventoryUITileEntity.GetAllInventoryUIs();
                foreach (InventoryUI inventoryUI in connections)
                {
                    if (!inventoryUI || !CanInsertIntoInventory(inventoryUI)) continue;
                    bool isInput = ReferenceEquals(inventoryUI, inputInventoryUI);
                    if (isInput && connections.Count > 1) continue;
                    
                    ItemSlotUtils.InsertInventoryIntoInventory(playerInventoryUI.GetInventory(),inventoryUI.GetInventory(),Global.MAX_SIZE);
                    inventoryUI.RefreshSlots();
                }
                RefreshPlayerInventoryUI();
            }

            void RefreshPlayerInventoryUI()
            {
                playerInventoryUI.RefreshSlots();
                originalPlayerInventoryUI.RefreshSlots();
            }
            
            bool CanInsertIntoInventory(InventoryUI inventoryUI)
            {
                List<ItemSlot> itemSlots = inventoryUI.GetInventory();
                if (itemSlots == null || itemSlots.Count == 0) return false;
                ItemState itemState = inventoryUI.GetItemSlotUI(0).ItemState;
                return itemState == ItemState.Solid; // Only need check the first item slot ui
            }
        }
        
        public void OnSelfDisplay(PlayerScript playerScript)
        {
            giveAllButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript, InventoryInteractionType.Give);
            });
            takeAllButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript, InventoryInteractionType.Take);
            });
            quickStackButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript, InventoryInteractionType.QuickStack);
            });
        }

        private enum InventoryInteractionType
        {
            Take,
            Give,
            QuickStack
        }

        private void InteractWithNearbyTileEntities(PlayerScript playerScript, InventoryInteractionType inventoryInteractionType)
        {
            ILoadedChunkSystem chunkSystem = playerScript.CurrentSystem;
            List<ItemSlot> playerInventory = playerInventoryUI.GetInventory();
            Vector2Int playerPosition = Global.GetCellPositionFromWorld(playerScript.transform.position);
            const int Range = 4;
            for (int r = 0; r <= Range; r++)
            {
                for (int x = -r; x <= r; x++)
                {
                    CheckSpot(x, r);
                }
    
                for (int y = r - 1; y >= -r; y--)
                {
                    CheckSpot(r, y);
                }
    
                for (int x = r - 1; x >= -r; x--)
                {
                    CheckSpot(x, -r);
                }
    
                for (int y = -r + 1; y <= r - 1; y++)
                {
                    CheckSpot(-r, y);
                }
            }
            playerInventoryUI.RefreshSlots();

            return;

            void CheckSpot(int x, int y)
            {
                Vector2Int position = new Vector2Int(x, y)+playerPosition;
                var (partition, positionInPartition) = chunkSystem.GetPartitionAndPositionAtCellPosition(position);
                ITileEntityInstance tileEntityInstance = partition.GetTileEntity(positionInPartition);
                List<ItemSlot> inputInventory = null;
                List<List<ItemSlot>> outputInventories = null;
                switch (tileEntityInstance)
                {
                    case ISingleSolidInventoryTileEntity solidInventoryTileEntity:
                    {
                        inputInventory = solidInventoryTileEntity.GetInventory();
                        outputInventories = new List<List<ItemSlot>> { inputInventory };
                        break;
                    }
                    case IMultiSolidItemStorageTileEntity multiSolidItemStorageTileEntity:
                        inputInventory = multiSolidItemStorageTileEntity.GetInputInventory();
                        outputInventories = multiSolidItemStorageTileEntity.GetOutputInventories();
                        break;
                    default:
                        return;
                }

                switch (inventoryInteractionType)
                {
                    case InventoryInteractionType.Take:
                        ItemSlotUtils.InsertInventoryIntoInventory(inputInventory,playerInventory,Global.MAX_SIZE);
                        break;
                    case InventoryInteractionType.Give:
                        foreach (List<ItemSlot> outputInventory in outputInventories)
                        {
                            ItemSlotUtils.InsertInventoryIntoInventory(playerInventory,outputInventory,Global.MAX_SIZE);
                        }
                        break;
                    case InventoryInteractionType.QuickStack:
                        ItemSlotUtils.QuickStackInventoryIntoInventory(inputInventory,playerInventory,Global.MAX_SIZE);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inventoryInteractionType), inventoryInteractionType, null);
                }

                if (tileEntityInstance is IInventoryListener inventoryListener)
                {
                    inventoryListener.InventoryUpdate(0);
                }
            }
        }
        
        
        

        public void SetBackgroundColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        
    }
}

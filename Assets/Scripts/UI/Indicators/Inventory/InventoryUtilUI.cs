using System;
using System.Collections.Generic;
using Chunks;
using Item.Display;
using Item.Slot;
using Items.Inventory;
using PlayerModule;
using TileEntity;
using TileEntity.Instances.Machine.UI;
using TMPro;
using UI.ToolTip;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player.UI.Inventory
{
    public class InventoryUtilUI : MonoBehaviour
    {
        private const string SORT_MODE_LOOKUP = "inventory_sort_mode";
        [SerializeField] private InventoryUI trashCanUI;
        [SerializeField] private Button takeAllButton;
        [SerializeField] private Button giveAllButton;
        [SerializeField] private Button quickStackButton;
        [SerializeField] private Button sortModeButton;

        private PlayerInventory playerInventory;
        private InventoryUI clonePlayerInventory;

        public void Start()
        {
            string TrashToolTipOverride(int index)
            {
                return "Trash Can";
            }
            trashCanUI.DisplayInventory(new List<ItemSlot>{null},false);
            trashCanUI.SetToolTipOverride(TrashToolTipOverride);
            ToolTipUIDisplayer uiDisplayer = takeAllButton.AddComponent<ToolTipUIDisplayer>();
            uiDisplayer.SetMessage("Take All");
            uiDisplayer = giveAllButton.AddComponent<ToolTipUIDisplayer>();
            uiDisplayer.SetMessage("Give All");
            uiDisplayer = quickStackButton.AddComponent<ToolTipUIDisplayer>();
            uiDisplayer.SetMessage("Quick Stack");
            uiDisplayer = sortModeButton.AddComponent<ToolTipUIDisplayer>();
            uiDisplayer.SetAction(GetSortModeTooltip);

            sortModeButton.GetComponentInChildren<TextMeshProUGUI>().text = GetSortModeText();
            string GetSortModeTooltip()
            {
                int current = PlayerPrefs.GetInt(SORT_MODE_LOOKUP);
                InventorySortingMode sortingMode = (InventorySortingMode)current;
                return $"Sort Mode: {sortingMode.ToString()}";
            }

            string GetSortModeText()
            {
                int current = PlayerPrefs.GetInt(SORT_MODE_LOOKUP);
                InventorySortingMode sortingMode = (InventorySortingMode)current;
                switch (sortingMode)
                {
                    case InventorySortingMode.Alphabetical:
                        return "a-Z";
                    case InventorySortingMode.Tier:
                        return "Tier";
                    case InventorySortingMode.ItemType:
                        return "Type";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            void IterateSortMode()
            {
                int current = PlayerPrefs.GetInt(SORT_MODE_LOOKUP);
                InventorySortingMode sortingMode = (InventorySortingMode)current;
                sortingMode = GlobalHelper.ShiftEnum(1, sortingMode);
                PlayerPrefs.SetInt(SORT_MODE_LOOKUP, (int)sortingMode);
                uiDisplayer.Refresh();
                sortModeButton.GetComponentInChildren<TextMeshProUGUI>().text = GetSortModeText();
            }

            sortModeButton.onClick.AddListener(IterateSortMode);

        }

        public void Update()
        {
            trashCanUI.SetItem(0,null);
        }
        
        public void SyncTileEntityUI(PlayerScript playerScript, InventoryUI playerInventoryClone, IInventoryUITileEntityUI inventoryUITileEntity)
        {
            playerInventory = playerScript.PlayerInventory;
            this.clonePlayerInventory = playerInventoryClone;
            bool inventoryConnected = inventoryUITileEntity != null;
            giveAllButton.gameObject.SetActive(inventoryConnected);
            giveAllButton.gameObject.SetActive(inventoryConnected);
            quickStackButton.gameObject.SetActive(inventoryConnected);
            if (!inventoryConnected) return;

            takeAllButton.onClick.AddListener(TakeAllPress);
            giveAllButton.onClick.AddListener(GiveAllPress);
            quickStackButton.onClick.AddListener(OnQuickStackPress);
            
            return;

            void OnQuickStackPress()
            {
                InventoryUI inputInventoryUI = inventoryUITileEntity.GetInput();
                if (!inputInventoryUI || !CanInsertIntoInventory(inputInventoryUI)) return;
                ItemSlotUtils.QuickStackInventoryIntoInventory(inputInventoryUI.GetInventory(),playerInventory.Inventory,Global.MAX_SIZE);
                inputInventoryUI.RefreshSlots();
                RefreshPlayerInventoryUI();
            }

            void GiveAllPress()
            {
                InventoryUI inputInventoryUI = inventoryUITileEntity.GetInput();
                if (!inputInventoryUI || !CanInsertIntoInventory(inputInventoryUI)) return;
                ItemSlotUtils.InsertInventoryIntoInventory(inputInventoryUI.GetInventory(),playerInventory.Inventory,Global.MAX_SIZE);
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
                    
                    ItemSlotUtils.InsertInventoryIntoInventory(playerInventory.Inventory,inventoryUI.GetInventory(),Global.MAX_SIZE);
                    inventoryUI.RefreshSlots();
                }
                RefreshPlayerInventoryUI();
            }

            void RefreshPlayerInventoryUI()
            {
                clonePlayerInventory?.RefreshSlots();
                playerInventory.InventoryUI.RefreshSlots();
            }
            
            bool CanInsertIntoInventory(InventoryUI inventoryUI)
            {
                List<ItemSlot> itemSlots = inventoryUI.GetInventory();
                if (itemSlots == null || itemSlots.Count == 0) return false;
                ItemState itemState = inventoryUI.GetItemSlotUI(0).ItemState;
                return itemState == ItemState.Solid; // Only need check the first item slot ui
            }
        }
        
        private enum InventoryInteractionType
        {
            Take,
            Give,
            QuickStack
        }

        public void DisplayWorldMode(PlayerScript playerScript)
        {
            playerInventory = playerScript.PlayerInventory;
            takeAllButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript,InventoryInteractionType.Take);
            });
            quickStackButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript,InventoryInteractionType.QuickStack);
            });
            giveAllButton.onClick.AddListener(() =>
            {
                InteractWithNearbyTileEntities(playerScript,InventoryInteractionType.Give);
            });
        }

        private void InteractWithNearbyTileEntities(PlayerScript playerScript, InventoryInteractionType inventoryInteractionType)
        {
            ILoadedChunkSystem chunkSystem = playerScript.CurrentSystem;
            List<ItemSlot> playerItemSlots = playerInventory.Inventory;
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
                        foreach (List<ItemSlot> outputInventory in outputInventories)
                        {
                            List<ItemSlot> takenItems = ItemSlotUtils.InsertInventoryIntoInventoryRecordItems(playerItemSlots,outputInventory,Global.MAX_SIZE);
                            if (takenItems.Count > 0) DisplayTransport(takenItems,position,TransportDirection.ToPlayer);
                        }
                        break;
                    case InventoryInteractionType.Give:
                        List<ItemSlot> given = ItemSlotUtils.InsertInventoryIntoInventoryRecordItems(inputInventory,playerItemSlots,Global.MAX_SIZE);
                        if (given.Count > 0) DisplayTransport(given,position,TransportDirection.FromPlayer);
                        break;
                    case InventoryInteractionType.QuickStack:
                        List<ItemSlot> quickStacked = ItemSlotUtils.QuickStackInventoryIntoInventoryRecordItems(inputInventory,playerItemSlots,Global.MAX_SIZE);
                        if (quickStacked.Count > 0) DisplayTransport(quickStacked,position,TransportDirection.FromPlayer);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(inventoryInteractionType), inventoryInteractionType, null);
                }

                if (tileEntityInstance is IInventoryListener inventoryListener)
                {
                    inventoryListener.InventoryUpdate(0);
                }
            }

            void DisplayTransport(List<ItemSlot> items, Vector2Int tileEntityPosition, TransportDirection transportDirection)
            {
                Vector2 worldPosition = Global.TILE_SIZE * Vector2.one * tileEntityPosition + Vector2.one*Global.TILE_SIZE/2f;
                const int MAX_DISPLAY = 3;
                GameObject transport = new GameObject("ItemTransport");
                ItemTransportWorldDisplay transportDisplay = transport.AddComponent<ItemTransportWorldDisplay>();
                
                switch (transportDirection)
                {
                    case TransportDirection.ToPlayer:
                        transportDisplay.DisplayTransport(playerScript,clonePlayerInventory,worldPosition,playerScript.transform.position,items,MAX_DISPLAY);
                        transportDisplay.SetMovingTarget(playerScript.transform);
                        break;
                    case TransportDirection.FromPlayer:
                        transportDisplay.DisplayTransport(playerScript,clonePlayerInventory,playerScript.transform.position,worldPosition,items,MAX_DISPLAY);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(transportDirection), transportDirection, null);
                }
            }
        }
        
        enum TransportDirection
        {
            ToPlayer,
            FromPlayer
        }
    }

    public enum InventorySortingMode
    {
        Alphabetical,
        Tier,
        ItemType
    }
    
}

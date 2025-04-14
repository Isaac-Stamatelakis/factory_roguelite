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
using Chunks.Systems;
using UnityEngine.UI;
using Entities;
using Item.Slot;
using Items.Tags;
using Items.Tags.FluidContainers;
using Newtonsoft.Json;
using Player;
using Player.Controls;
using Player.Inventory;
using Player.Tool;
using Player.UI.Inventory;
using PlayerModule.KeyPress;
using PlayerModule.Mouse;
using Robot;
using Robot.Tool;
using Robot.Tool.UI;
using TileEntity;
using UI;
using UI.Indicators;
using UI.Indicators.General;
using UI.ToolTip;

namespace PlayerModule {
    public class PlayerInventory : MonoBehaviour, IIndexInventoryListener
    {
        public enum InventoryMode
        {
            Open,
            Closed
        }
        public const int INVENTORY_SIZE = 10;
        
        [SerializeField] private InventoryUI playerInventoryGrid;
        [SerializeField] private PlayerToolListUI playerToolListUI;
        private PlayerRobot playerRobot;
        private PlayerMouse playerMouse;
        private InteractMode mode = InteractMode.Inventory;
        private int selectedSlot = 0;
        private int selectedTool = 0;
        public int CurrentToolIndex => selectedTool;
        private static UnityEngine.Vector2Int inventorySize = new UnityEngine.Vector2Int(10,4);
        private PlayerInventoryData playerInventoryData;
        private GameObject inventoryContainer;
        private GameObject inventoryItemContainer;
        private GameObject hotbarNumbersContainer;
        public PlayerInventoryData PlayerInventoryData => playerInventoryData;
        public List<ItemSlot> Inventory => playerInventoryData.Inventory;
        public InventoryUI InventoryUI => playerInventoryGrid;
        public IRobotToolInstance CurrentTool => playerRobot.RobotTools[selectedTool];
        public RobotToolType CurrentToolType => playerRobot.ToolTypes[selectedTool];
        public PlayerToolListUI PlayerRobotToolUI => playerToolListUI;
        private CanvasController canvasController;
        private InventoryMode inventoryMode = InventoryMode.Closed;

        private GameObject inventoryIndicator;
        // Start is called before the first frame update
        void Start()
        {
            playerRobot = GetComponent<PlayerRobot>();
            playerMouse = GetComponent<PlayerMouse>();
            canvasController = CanvasController.Instance;
        }

        public void Initialize(string json) {
            playerInventoryData = PlayerInventoryFactory.DeserializePlayerInventory(json);
            playerInventoryGrid.AddListener(this);
            DisplayInventory();
        }
        
        
        public void InitializeToolDisplay()
        {
            playerToolListUI.Initialize(playerRobot.RobotTools, GetComponent<PlayerScript>());
        }

        public void Refresh()
        {
            playerInventoryGrid.RefreshSlots();
        }
        
        
        void Update()
        {
            if (canvasController.BlockKeyInput) return;
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                mode = InteractMode.Tools;
                playerToolListUI.Highlight(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                mode = InteractMode.Inventory;
                playerToolListUI.Highlight(false);
            }
        }
        
        public void ChangeSelectedSlot(int slot) {
            switch (mode)
            {
                case InteractMode.Inventory:
                    selectedSlot = slot;
                    playerInventoryGrid.HighlightSlot(slot);
                    ItemSlot itemSlot = playerInventoryGrid.GetItemSlot(selectedSlot);
                    
                    IndicatorManager indicatorManager = GetComponent<PlayerScript>().PlayerUIContainer.IndicatorManager;
                    indicatorManager.RemovePlaceBundles();
                    if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
                    if (itemSlot.itemObject is TileItem)
                    {
                        indicatorManager.AddViewBundle(IndicatorDisplayBundle.TilePlace);
                    } else if (itemSlot.itemObject is ConduitItem)
                    {
                        indicatorManager.AddViewBundle(IndicatorDisplayBundle.ConduitPlace);
                    }
                    break;
                case InteractMode.Tools:
                    ChangeSelectedTool(slot % playerRobot.RobotTools.Count);
                    break;
            }
            
        }

        public void ToggleInventoryMode()
        {
            inventoryMode = GlobalHelper.ShiftEnum(1, inventoryMode);
            DisplayInventory();

        }

        private void DisplayInventory()
        {
            PlayerScript playerScript = GetComponent<PlayerScript>();
            switch (inventoryMode)
            {
                case InventoryMode.Open:
                    playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory,4*INVENTORY_SIZE);
                    playerInventoryGrid.HighlightSlot(selectedSlot);

                    if (!inventoryIndicator)
                    {
                        inventoryIndicator = GameObject.Instantiate(
                            playerScript.PlayerUIContainer.InventoryIndicatorPrefab,
                            playerScript.PlayerUIContainer.IndicatorContainer
                        );
                        inventoryIndicator.transform.SetAsFirstSibling();
                        inventoryIndicator.GetComponentInChildren<InventoryUtilUI>().DisplayWorldMode(playerScript);
                    }
                    
                    break;
                case InventoryMode.Closed:
                    playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory,INVENTORY_SIZE);
                    playerInventoryGrid.HighlightSlot(selectedSlot);
                    if (inventoryIndicator)
                    {
                        ToolTipController.Instance.HideToolTip();
                        GameObject.Destroy(inventoryIndicator);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            List<string> topText = new List<string>();
            for (int i = 0; i < INVENTORY_SIZE; i++)
            {
                topText.Add(((i+1)%10).ToString());
            }
            
            playerInventoryGrid.DisplayTopText(topText);

            void FreezeTopText(ItemSlotUI itemSlotUI)
            {
                itemSlotUI.LockTopText = true;
            }
            playerInventoryGrid.ApplyFunctionToAllSlots(FreezeTopText);
        }

        public void ChangeSelectedTool(int index)
        {
            selectedTool = index;
            playerMouse.UpdateOnToolChange();
            playerToolListUI.HighLightTool(selectedTool);
        }
        
        
        public void deiterateInventoryAmount() {
            ItemSlot itemInventoryData = playerInventoryData.Inventory[selectedSlot];
            if (itemInventoryData == null) {
                return;
            }
            InventoryUpdate(0);
            playerInventoryData.Inventory[selectedSlot].amount--;
            playerInventoryGrid.DisplayItem(selectedSlot);
        }

        public void IterateSelectedTile(int iterator) {
            switch (mode)
            {
                case InteractMode.Inventory:
                    selectedSlot += iterator;
                    selectedSlot = (int) Global.ModInt(selectedSlot,INVENTORY_SIZE);
                    ChangeSelectedSlot(selectedSlot); 
                    break;
                case InteractMode.Tools:
                    selectedTool += iterator;
                    selectedTool = (int) Global.ModInt(selectedTool,playerRobot.RobotTools.Count);
                    ChangeSelectedSlot(selectedTool); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public void Give(ItemSlot itemSlot) {
            if (ItemSlotUtils.CanInsertIntoInventory(playerInventoryData.Inventory, itemSlot, Global.MAX_SIZE))
            {
                ItemSlotUtils.InsertIntoInventory(playerInventoryData.Inventory,itemSlot,Global.MAX_SIZE);
                Refresh();
                return;
            }
            
            IChunk chunk = DimensionManager.Instance.GetPlayerSystem().GetChunk(Global.GetChunkFromWorld(transform.position));
            if (chunk is not ILoadedChunk loadedChunk) return;
            
            ItemEntityFactory.SpawnItemEntityWithRandomVelocity(transform.position,itemSlot,loadedChunk.GetEntityContainer());
        }

        public void GiveFluid(FluidTileItem fluidTileItem, float fill)
        {
            if (!fluidTileItem) return;
            const int FLUID_IN_TILE = 1000;
            uint fluidAmount = (uint)fill * FLUID_IN_TILE;
            int firstEmptyCellIndex = -1;
            for (var index = 0; index < playerInventoryData.Inventory.Count; index++)
            {
                
                var itemSlot = playerInventoryData.Inventory[index];
                if (fluidAmount == 0) return;
                if (ItemSlotUtils.IsItemSlotNull(itemSlot)) continue;
                if (itemSlot.itemObject is not IFluidContainerDataItem fluidCellItem) continue;
                
                if (itemSlot.tags?.Dict == null || !itemSlot.tags.Dict.TryGetValue(ItemTag.FluidContainer, out var tagData))
                {
                    if (firstEmptyCellIndex < 0) firstEmptyCellIndex = index;
                    continue;
                }
                ItemSlot fluidSlot = tagData as ItemSlot;
                if (ItemSlotUtils.IsItemSlotNull(fluidSlot)) continue; // Should never be null but to be safe
                if (!string.Equals(fluidSlot.itemObject.id, fluidTileItem.id)) continue;
                if (itemSlot.amount == 1 && fluidSlot.amount + fluidAmount <= fluidCellItem.storage)
                {
                    ItemSlotUtils.InsertIntoSlot(fluidSlot, ref fluidAmount, fluidCellItem.storage);
                    if (fluidSlot.amount == fluidCellItem.storage)
                    {
                        playerInventoryData.Inventory[index] = null;
                        Give(itemSlot);
                        return;
                    }
                    playerInventoryGrid.DisplayItem(index);
                    continue;
                }
                if (fluidSlot.amount + fluidAmount > fluidCellItem.storage) continue;
                ItemSlot spliced = ItemSlotFactory.Splice(itemSlot, 1);
                itemSlot.amount--;
                ItemSlot splicedFluidSlot = spliced.tags.Dict[ItemTag.FluidContainer] as ItemSlot;
                ItemSlotUtils.InsertIntoSlot(splicedFluidSlot, ref fluidAmount, fluidCellItem.storage);
                Give(spliced);
                
            }

            if (firstEmptyCellIndex > 0)
            {
                ItemSlot itemSlot = playerInventoryData.Inventory[firstEmptyCellIndex];
                IFluidContainerDataItem fluidCellItem = (IFluidContainerDataItem)itemSlot.itemObject;
                uint amount = fluidAmount > fluidCellItem.storage ? fluidCellItem.storage : fluidAmount;
                ItemSlot newFluidSlot = new ItemSlot(fluidTileItem, amount, null);
                if (itemSlot.amount == 1)
                {
                    ItemSlotUtils.AddTag(itemSlot, ItemTag.FluidContainer, newFluidSlot);
                    playerInventoryGrid.DisplayItem(firstEmptyCellIndex);
                    return;
                }
                ItemSlot spliced = ItemSlotFactory.Splice(itemSlot, 1);
                itemSlot.amount--;
                ItemSlotUtils.AddTag(spliced, ItemTag.FluidContainer, newFluidSlot);
                Give(spliced);
            }
        }
        

        public void GiveItems(List<ItemSlot> itemSlots)
        {
            if (itemSlots == null) return;
            foreach (ItemSlot itemSlot in itemSlots)
            {
                Give(itemSlot);
            }
        }

        public void DropAll()
        {
            Vector2 position = transform.position;
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.GetPlayerSystem();
            Vector2Int chunkPosition = new Vector2Int(Mathf.FloorToInt(position.x/(2*Global.CHUNK_SIZE)), Mathf.FloorToInt(position.y/(2*Global.CHUNK_SIZE)));
            IChunk chunk = closedChunkSystem.GetChunk(chunkPosition);
            if (chunk is ILoadedChunk loadedChunk)
            {
                TileEntityUtils.spawnItemsOnBreak(Inventory,transform.position,loadedChunk);
            }
            for (int i = 0; i < Inventory.Count; i++)
            {
                Inventory[i] = null;
            }
            Refresh();
        }

        public string GetSelectedId()
        {
            if (ItemSlotUtils.IsItemSlotNull(playerInventoryData.Inventory[selectedSlot])) return null;
            return playerInventoryData.Inventory[selectedSlot].itemObject.id;
        }

        public ItemSlot GetSelectedItemSlot()
        {
            return playerInventoryData.Inventory[selectedSlot];
        }

        public ItemSlot getSelectedItemSlot() {
            return playerInventoryData.Inventory[selectedSlot];
        }

        public void removeSelectedItemSlot() {
            InventoryUpdate(0);
            playerInventoryData.Inventory[selectedSlot] = null;
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

    public class PlayerInventoryData
    {
        public List<ItemSlot> Inventory;
        
        public PlayerInventoryData(List<ItemSlot> inventory)
        {
            Inventory = inventory;
        }
    }

    public static class PlayerInventoryFactory
    {
        public static string Serialize(PlayerInventoryData playerInventory)
        {
            string sInventory = ItemSlotFactory.serializeList(playerInventory.Inventory);
            
            SerializedPlayerInventory serializedPlayerInventory = new SerializedPlayerInventory(sInventory);
            return JsonConvert.SerializeObject(serializedPlayerInventory);
        }

        public static PlayerInventoryData DeserializePlayerInventory(string json)
        {
            if (json == null) return GetDefault();
            try
            {
                SerializedPlayerInventory sPlayerInventoryData = JsonConvert.DeserializeObject<SerializedPlayerInventory>(json);
                List<ItemSlot> inventory = ItemSlotFactory.Deserialize(sPlayerInventoryData.SInventory);
                if (inventory == null) inventory = GetDefaultItemInventory();
                return new PlayerInventoryData(inventory);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return GetDefault();
            }
        }

        public static PlayerInventoryData GetDefault()
        {
            return new PlayerInventoryData(GetDefaultItemInventory());
        }

        public static List<ItemSlot> GetDefaultItemInventory()
        {
            return ItemSlotFactory.createEmptyInventory(40);
        }

        private class SerializedPlayerInventory
        {
            public string SInventory;

            public SerializedPlayerInventory(string sInventory)
            {
                SInventory = sInventory;
            }
        }
    }

    public interface IPlayerInventoryIntegratedUI {
        
    }

    public enum InteractMode {
        Inventory,
        Tools
    }
}
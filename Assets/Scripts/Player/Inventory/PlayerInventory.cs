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
using Newtonsoft.Json;
using Player;
using Player.Inventory;
using Player.Tool;
using Robot;
using Robot.Tool;
using TileEntity;

namespace PlayerModule {
    public class PlayerInventory : MonoBehaviour, IInventoryListener
    {
        public static readonly int COLUMNS = 10;
        [SerializeField] private PlayerRobot playerRobot;
        [SerializeField] private InventoryUI playerInventoryGrid;
        [SerializeField] private PlayerToolListUI playerToolListUI;
        private PlayerPickUp playerPickUp;
        private InventoryDisplayMode mode = InventoryDisplayMode.Inventory;
        private static int entityLayer;
        private int selectedSlot = 0;
        private int selectedTool = 0;
        private static UnityEngine.Vector2Int inventorySize = new UnityEngine.Vector2Int(10,4);
        private PlayerInventoryData playerInventoryData;
        private GameObject inventoryContainer;
        private GameObject inventoryItemContainer;
        private GameObject hotbarNumbersContainer;
        private bool expanded = false;
        public PlayerInventoryData PlayerInventoryData => playerInventoryData;
        public List<ItemSlot> Inventory => playerInventoryData.Inventory;
        public InventoryUI InventoryUI => playerInventoryGrid;
        public IRobotToolInstance CurrentTool => playerRobot.RobotTools[selectedTool];
        public RobotToolType CurrentToolType => playerRobot.ToolTypes[selectedTool];
        public InventoryDisplayMode Mode => mode;
        public PlayerToolListUI PlayerRobotToolUI => playerToolListUI;
        // Start is called before the first frame update
        void Start()
        {
            entityLayer = 1 << LayerMask.NameToLayer("Entity");
            playerPickUp = GetComponentInChildren<PlayerPickUp>();
        }

        public void Initialize() {
            playerInventoryData = PlayerInventoryFactory.DeserializePlayerInventory(GetComponent<PlayerIO>().GetPlayerInventoryData());
            playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory,10);
            playerInventoryGrid.HighlightSlot(0);
            playerInventoryGrid.AddListener(this);
            
        }

        public void InitializeToolDisplay()
        {
            playerToolListUI.Initialize(playerRobot.RobotTools);
        }

        public void Refresh()
        {
            playerInventoryGrid.RefreshSlots();
        }
        
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                mode = InventoryDisplayMode.Tools;
                playerToolListUI.Highlight(true);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                mode = InventoryDisplayMode.Inventory;
                playerToolListUI.Highlight(false);
            }
        }

        public void toggleInventory() {
            expanded = !expanded;
            if (expanded) {
                playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory);
            } else {
                playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory,10);
            }
        }
        public void ChangeSelectedSlot(int slot) {
            switch (mode)
            {
                case InventoryDisplayMode.Inventory:
                    selectedSlot = slot;
                    playerInventoryGrid.HighlightSlot(slot);
                    break;
                case InventoryDisplayMode.Tools:
                    selectedTool = slot % playerRobot.RobotTools.Count;
                    playerToolListUI.IterateSelectedTool(selectedTool);
                    break;
            }
            
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
                case InventoryDisplayMode.Inventory:
                    selectedSlot += iterator;
                    selectedSlot = (int) Global.modInt(selectedSlot,COLUMNS);
                    ChangeSelectedSlot(selectedSlot); 
                    break;
                case InventoryDisplayMode.Tools:
                    selectedTool += iterator;
                    selectedTool = (int) Global.modInt(selectedTool,playerRobot.RobotTools.Count);
                    ChangeSelectedSlot(selectedTool); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        public void Give(ItemSlot itemSlot) {
            if (ItemSlotUtils.CanInsertIntoInventory(playerInventoryData.Inventory, itemSlot, Global.MaxSize))
            {
                ItemSlotUtils.InsertIntoInventory(playerInventoryData.Inventory,itemSlot,Global.MaxSize);
                Refresh();
                return;
            }
            IChunk chunk = DimensionManager.Instance.getPlayerSystem(transform).getChunk(Global.getCellPositionFromWorld(transform.position));
            if (chunk is not ILoadedChunk loadedChunk) return;
            ItemEntityHelper.spawnItemEntity(transform.position,itemSlot,loadedChunk.getEntityContainer());
        }

        public void DropAll()
        {
            Vector2 position = transform.position;
            ClosedChunkSystem closedChunkSystem = DimensionManager.Instance.getPlayerSystem(transform);
            Vector2Int chunkPosition = new Vector2Int(Mathf.FloorToInt(position.x/(2*Global.ChunkSize)), Mathf.FloorToInt(position.y/(2*Global.ChunkSize)));
            IChunk chunk = closedChunkSystem.getChunk(chunkPosition);
            if (chunk is ILoadedChunk loadedChunk)
            {
                TileEntityHelper.spawnItemsOnBreak(Inventory,transform.position,loadedChunk,closedChunkSystem);
            }
            for (int i = 0; i < Inventory.Count; i++)
            {
                Inventory[i] = null;
            }
            Refresh();
        }

        public string getSelectedId()
        {
            if (ItemSlotUtils.IsItemSlotNull(playerInventoryData.Inventory[selectedSlot])) return null;
            return playerInventoryData.Inventory[selectedSlot].itemObject.id;
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
            playerPickUp.TryPickUpAllCollided();
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

    public enum InventoryDisplayMode {
        Inventory,
        Tools
    }
}
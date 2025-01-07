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
using Newtonsoft.Json;
using Player.Tool;

namespace PlayerModule {
    public class PlayerInventory : MonoBehaviour, IInventoryListener
    {
        public static readonly int COLUMNS = 10;
        [SerializeField] private PlayerRobot playerRobot;
        [SerializeField] private InventoryUI playerInventoryGrid;
        [SerializeField] private PlayerToolListUI playerToolListUI;
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
        public List<PlayerTool> PlayerTools => playerInventoryData.PlayerTools;
        
        public InventoryUI InventoryUI => playerInventoryGrid;
        // Start is called before the first frame update
        void Start()
        {
            entityLayer = 1 << LayerMask.NameToLayer("Entity");
        }

        public void Initalize() {
            GetComponent<PlayerIO>().initRead();
            playerInventoryData = PlayerInventoryFactory.DeserializePlayerInventory(GetComponent<PlayerIO>().getPlayerInventoryData());
            playerInventoryGrid.DisplayInventory(playerInventoryData.Inventory,10);
            playerToolListUI.Initialize(playerInventoryData.PlayerTools);
        }

        public void Refresh()
        {
            playerInventoryGrid.RefreshSlots();
        }
        
        
        void Update()
        {
            raycastHitTileEntities();
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                mode = (InventoryDisplayMode)(((int)mode + 1) % Enum.GetValues(typeof(InventoryDisplayMode)).Length);
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
                    selectedTool = slot;
                    playerToolListUI.IterateSelectedTool(selectedTool);
                    break;
            }
            
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
                    for (int n = playerInventoryData.Inventory.Count-1; n >= 0; n --) {
                        ItemSlot inventorySlot = playerInventoryData.Inventory[n];
                        if (ItemSlotUtils.IsItemSlotNull(inventorySlot)) {
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
                        playerInventoryData.Inventory[firstOpenSlot] = itemEntityProperities.itemSlot;
                        Destroy(itemEntityProperities.gameObject);
                        if (firstOpenSlot < inventorySize.x * inventorySize.y) {
                            playerInventoryGrid.SetItem(firstOpenSlot, playerInventoryData.Inventory[firstOpenSlot]);
                        }
                    }
                }
            }
        }
        

        public void deiterateInventoryAmount() {
            ItemSlot itemInventoryData = playerInventoryData.Inventory[selectedSlot];
            if (itemInventoryData == null) {
                return;
            }
            playerInventoryData.Inventory[selectedSlot].amount--;
            playerInventoryGrid.DisplayItem(selectedSlot);
        }

        public void iterateSelectedTile(int iterator) {
            selectedSlot += iterator;
            selectedSlot = (int) Global.modInt(selectedSlot,COLUMNS);
            ChangeSelectedSlot(selectedSlot); 
        }

        public void give(ItemSlot itemSlot) {
            if (!ItemSlotUtils.CanInsertIntoInventory(playerInventoryData.Inventory,itemSlot,Global.MaxSize)) {
                IChunk chunk = DimensionManager.Instance.getPlayerSystem(transform).getChunk(Global.getCellPositionFromWorld(transform.position));
                if (chunk is not ILoadedChunk loadedChunk) {
                    return;
                }
                ItemEntityHelper.spawnItemEntity(transform.position,itemSlot,loadedChunk.getEntityContainer());
            } else {
                ItemSlotUtils.InsertIntoInventory(playerInventoryData.Inventory,itemSlot,Global.MaxSize);
            }
        }

        public string getSelectedId() {
            if (playerInventoryData.Inventory == null) {
                return null;
            }
            if (playerInventoryData.Inventory[selectedSlot] == null || playerInventoryData.Inventory[selectedSlot].itemObject == null) {
                return null;
            }
            return playerInventoryData.Inventory[selectedSlot].itemObject.id;
        }

        public ItemSlot getSelectedItemSlot() {
            return playerInventoryData.Inventory[selectedSlot];
        }

        public void removeSelectedItemSlot() {
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
        public List<PlayerTool> PlayerTools;

        public PlayerInventoryData(List<ItemSlot> inventory, List<PlayerTool> playerTools)
        {
            Inventory = inventory;
            PlayerTools = playerTools;
        }
    }

    public static class PlayerInventoryFactory
    {
        public static string Serialize(PlayerInventoryData playerInventory)
        {
            string sInventory = ItemSlotFactory.serializeList(playerInventory.Inventory);
            List<int> toolTypes = new List<int>();
            List<string> sTools = PlayerToolFactory.SerializeList(playerInventory.PlayerTools);
            foreach (PlayerTool playerTool in playerInventory.PlayerTools)
            {
                toolTypes.Add((int)PlayerToolFactory.GetType(playerTool));
                sTools.Add(JsonConvert.SerializeObject(playerTool));
            }

            string sToolTypes = JsonConvert.SerializeObject(toolTypes);
            SerializedPlayerInventory serializedPlayerInventory = new SerializedPlayerInventory(sInventory, sTools, sToolTypes);
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
                List<PlayerTool> tools = PlayerToolFactory.Deserialize(sPlayerInventoryData.STools,sPlayerInventoryData.SToolTypes);
                return new PlayerInventoryData(inventory, tools);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogWarning(e);
                return GetDefault();
            }
        }

        public static PlayerInventoryData GetDefault()
        {
            return new PlayerInventoryData(GetDefaultItemInventory(), PlayerToolFactory.GetDefaults());
        }

        public static List<ItemSlot> GetDefaultItemInventory()
        {
            return ItemSlotFactory.createEmptyInventory(40);
        }

        private class SerializedPlayerInventory
        {
            public string SInventory;
            public List<string> STools;
            public string SToolTypes;

            public SerializedPlayerInventory(string sInventory, List<string> sTools, string sToolTypes)
            {
                SInventory = sInventory;
                STools = sTools;
                SToolTypes = sToolTypes;
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
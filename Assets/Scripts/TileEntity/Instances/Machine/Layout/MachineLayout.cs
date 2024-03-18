using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using TileEntityModule;

namespace ItemModule.Inventory {

    public enum InventoryUIMode {
        Recipe,
        Standard
    }
    public interface IDisplayableLayout<Inv> where Inv : TileEntityInventory {
        public abstract void display(Transform parent, Inv inventory, InventoryUIMode uIType);
    }
    public abstract class InventoryLayout : ScriptableObject {
        
    }

    public abstract class TypedInventoryLayout<Inv> : InventoryLayout, IDisplayableLayout<Inv> where Inv : TileEntityInventory
    {
        public abstract void display(Transform parent, Inv inventory, InventoryUIMode uIType);
    }



    /// <summary>
    /// An Inventory layout is a collection of slots which belong to the same list in a tile entity
    /// </summary>
    [System.Serializable]
    public class AdvancedInventoryLayout {
        public List<Vector2Int> slots;
        public ItemState state;
        public InventoryType inventoryType;
        public string inventoryName;
    }

    public class Inventory {
        private List<ItemSlot> slots;
        private ItemState state;
        public List<ItemSlot> Slots { get => slots; set => slots = value; }
        public ItemState State { get => state; set => state = value; }
        public bool isEmpty() {
            foreach (ItemSlot slot in slots) {
                if (slot != null || slot.itemObject != null) {
                    return false;
                }
            }
            return true;
        }

        public Inventory(List<ItemSlot> itemSlots) {
            this.slots = itemSlots;
        }
    }

    public static class InventoryFactory {
        public static Inventory deserialize(string data) {
            List<ItemSlot> itemSlots = ItemSlotFactory.deserialize(data);
            return new Inventory(itemSlots);
        }

        public static string serialize(Inventory inventory) {
            if (inventory == null) {
                return null;
            }
            return ItemSlotFactory.serializeList(inventory.Slots);
        }
    }

    public class InventoryCollection {
        private Dictionary<string, Inventory> inventories;
        public InventoryCollection(Dictionary<string,Inventory> inventories) {
            this.inventories = inventories;
        }

        public Dictionary<string, Inventory> Inventories { get => inventories; set => inventories = value; }
        public List<ItemSlot> getAll() {
            List<ItemSlot> total = new List<ItemSlot>();
            foreach (Inventory inventory in inventories.Values) {
                total.AddRange(inventory.Slots);
            }
            return total;
        }
        public List<ItemSlot> getInventorySlots(string inventoryName) {
            if (inventories.ContainsKey(inventoryName)) {
                return inventories[inventoryName].Slots;
            }
            Debug.LogError("Inventory collection does not contain inventory '" + inventoryName + "'");
            return null;
        }
    }

    public static class InventoryCollectionHelper {
        public static string serialize(InventoryCollection inventoryCollection) {
            Dictionary<string, string> serializedInventories = new Dictionary<string, string>();
            foreach (KeyValuePair<string, Inventory> kvp in inventoryCollection.Inventories) {
                serializedInventories.Add(kvp.Key,ItemSlotFactory.serializeList(kvp.Value.Slots));
            }
            return JsonConvert.SerializeObject(serializedInventories);
        } 

        public static InventoryCollection deserialize(string data) {
            Dictionary<string, string> serializedInventories = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            Dictionary<string, Inventory> inventories = new Dictionary<string, Inventory>();
            foreach (KeyValuePair<string,string> kvp in serializedInventories) {
                inventories[kvp.Key] = new Inventory(ItemSlotFactory.deserialize(kvp.Value));
            }
            return new InventoryCollection(inventories);
        }
    }
    public enum InventoryType {
        Input,
        Output,
        Other
    }

}


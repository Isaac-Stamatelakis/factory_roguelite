using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;

namespace Items {
    public class ItemHashTable
    {
        private Dictionary<string,Dictionary<ItemTagKey, ItemSlot>> idTagHashDict;
        public ItemHashTable() {
            idTagHashDict = new Dictionary<string, Dictionary<ItemTagKey, ItemSlot>>();
        }

        
        public int getCount() {
            int count = 0;
            foreach (Dictionary<ItemTagKey,ItemSlot> val in idTagHashDict.Values) {
                count += val.Count;
            }
            return count;
        }

        public void addItem(ItemSlot itemSlot, bool nullSafe = true, ItemTagKey itemTagKey = null) {
            if (nullSafe && (itemSlot == null || itemSlot.itemObject == null)) {
                return;
            }
            if (itemTagKey == null) {
                itemTagKey = new ItemTagKey(itemSlot.tags);
            }
            addItem(itemSlot,itemSlot.itemObject.id,itemTagKey);
        }
        public void addItem(ItemSlot itemSlot, string id, ItemTagKey itemTagKey) {
            if (!idTagHashDict.ContainsKey(id)) {
                idTagHashDict[id] = new Dictionary<ItemTagKey, ItemSlot>();
            }
            if (!idTagHashDict[id].ContainsKey(itemTagKey)) {
                idTagHashDict[id][itemTagKey] = itemSlot;
            } else {
                idTagHashDict[id][itemTagKey].amount += itemSlot.amount;
            }
            
        }
        public bool containsItem(ItemSlot itemSlot, bool nullSafe = true, ItemTagKey itemTagKey = null) {
            if (nullSafe && (itemSlot == null || itemSlot.itemObject == null)) {
                return false;
            }
            if (itemTagKey == null) {
                itemTagKey = new ItemTagKey(itemSlot.tags);
            }
            return containsItem(itemSlot.itemObject.id,itemTagKey);
        }
        public bool containsItem(string id, ItemTagKey itemTagKey) {
            if (!idTagHashDict.ContainsKey(id)) {
                return false;
            }
            return idTagHashDict[id].ContainsKey(itemTagKey);
        }
        public void removeItem(ItemSlot itemSlot, bool nullSafe = true, ItemTagKey itemTagKey = null) {
            if (nullSafe && (itemSlot == null || itemSlot.itemObject == null)) {
                return;
            }
            if (itemTagKey == null) {
                itemTagKey = new ItemTagKey(itemSlot.tags);
            }
            removeItem(itemSlot.itemObject.id,itemTagKey);
        }
        public void removeItem(string id, ItemTagKey itemTagKey) {
            if (!containsItem(id,itemTagKey)) {
                return;
            }
            idTagHashDict[id].Remove(itemTagKey);
            if (idTagHashDict.Count == 0) {
                idTagHashDict.Remove(id);
            }
        }
        public ItemHashTable intersect(ItemHashTable itemHashTable) {
            ItemHashTable newHashTable = new ItemHashTable();
            foreach (KeyValuePair<string,Dictionary<ItemTagKey,ItemSlot>> kvp in idTagHashDict) {
                string id = kvp.Key;
                foreach (KeyValuePair<ItemTagKey,ItemSlot> kvp1 in kvp.Value) {
                    ItemTagKey tagKey = kvp1.Key;
                    if (itemHashTable.containsItem(id,tagKey)) {
                        newHashTable.addItem(getItem(id,tagKey));
                        newHashTable.addItem(itemHashTable.getItem(id,tagKey));
                    }
                }
            }
            return newHashTable;
        }
        public ItemSlot getItem(string id, ItemTagKey itemTagKey) {
            if (!containsItem(id,itemTagKey)) {
                return null;
            }
            return idTagHashDict[id][itemTagKey];
        }

        public List<ItemSlot> GetAllItems()
        {
            List<ItemSlot> itemSlots = new List<ItemSlot>();
            foreach (var (id, dict) in idTagHashDict)
            {
                foreach (var (tag, slot) in dict)
                {
                    itemSlots.Add(getItem(id,tag));
                }
            }

            return itemSlots;
        }
    }

}

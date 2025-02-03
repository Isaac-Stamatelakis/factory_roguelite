using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
namespace Items {
    public static class LootTableUtils {
        public static List<ItemSlot> Open(LootTable lootTable) {
            int amount = Random.Range(lootTable.MinItems,lootTable.MaxItems+1);
            return OpenWithAmount(lootTable,amount);
        }

        public static List<ItemSlot> OpenWithAmount(LootTable lootTable, int amount) {
            List<ItemSlot> itemSlots = new List<ItemSlot>();
            HashSet<string> excludedIds = new HashSet<string>();
            for (int i = 0; i < amount; i++) {
                LootResult lootResult = OpenLootTable(lootTable,excludedIds);
                if (lootResult == null) continue;
                
                if (!lootTable.repetitions) {
                    excludedIds.Add(lootResult.item.id);
                }
                uint itemSlotAmount = (uint)Random.Range(lootResult.MinAmount,lootResult.MaxAmount);
                itemSlots.Add(new ItemSlot(lootResult.item,itemSlotAmount,null));
            }
            return itemSlots;
        }

        private static LootResult OpenLootTable(LootTable lootTable, HashSet<string> excludedIDs) {
            int frequencySum = 0;
            foreach (LootResult lootResult in lootTable.loot) {
                if (!lootTable) {
                    continue;
                }
                if (!lootTable.repetitions && excludedIDs.Contains(lootResult.item.id)) {
                    continue;
                }
                frequencySum += lootResult.frequency;
            }
            int ran = Random.Range(0,frequencySum);
            frequencySum = 0;
            foreach (LootResult lootResult in lootTable.loot) {
                
                if (ReferenceEquals(lootResult.item,null)) {
                    continue;
                }
                if (!lootTable.repetitions && excludedIDs.Contains(lootResult.item.id)) { 
                    continue;
                }
                frequencySum += lootResult.frequency;
                if (frequencySum >= ran) {
                    return lootResult;
                }
            }   
            Debug.LogWarning("LootTableUtils returned null for " + lootTable.name);
            return null;
        }

        
    }
}
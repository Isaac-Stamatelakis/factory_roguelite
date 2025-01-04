using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
namespace Items {
    public static class LootTableHelper {
        public static List<ItemSlot> open(LootTable lootTable) {
            int lowRange = lootTable.lootRange.x;
            int upperRange = lootTable.lootRange.y;
            int amount = Random.Range(lowRange,upperRange+1);
            return openWithAmount(lootTable,amount);
        }

        public static List<ItemSlot> openWithAmount(LootTable lootTable, int amount) {
            List<ItemSlot> itemSlots = new List<ItemSlot>();
            HashSet<string> excludedIds = new HashSet<string>();
            for (int i = 0; i < amount; i++) {
                LootResult lootResult = openLootTable(lootTable,excludedIds);
                if (!lootTable.repetitions) {
                    excludedIds.Add(lootResult.item.id);
                }
                uint itemSlotAmount = (uint)Random.Range(lootResult.amountRange.x,lootResult.amountRange.y);
                itemSlots.Add(new ItemSlot(lootResult.item,itemSlotAmount,null));
            }
            return itemSlots;
        }

        private static LootResult openLootTable(LootTable lootTable, HashSet<string> excludedIDs) {
            int frequencySum = 0;
            foreach (LootResult lootResult in lootTable.loot) {
                if (lootResult.item == null) {
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
                
                if (lootResult.item == null) {
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
            Debug.LogWarning("LootTableHelper returned null for " + lootTable.name);
            return null;
        }

        
    }
}
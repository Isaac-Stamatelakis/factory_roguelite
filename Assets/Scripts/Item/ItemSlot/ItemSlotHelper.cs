using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags;

public static class ItemSlotHelper
{
    public static bool insertIntoInventory(List<ItemSlot> contained, ItemSlot toInsert) {
        
        for (int i = 0; i < contained.Count; i++) {
            ItemSlot inputSlot = contained[i];
            if (inputSlot == null || inputSlot.itemObject == null) {
                contained[i] = new ItemSlot(toInsert.itemObject,toInsert.amount,toInsert.tags);
                toInsert.amount=0;
                return true;
            }
            if (!ItemSlotHelper.areEqualNoNullCheck(inputSlot,toInsert)) {
                continue;
            }
            
            if (inputSlot.amount >= Global.MaxSize) {
                continue;
            }
            // Success
            combineItems(inputSlot,toInsert);
            return true;
        }
        return false;
    }

    public static void combineItems(ItemSlot a, ItemSlot b) {
        int sum = a.amount + b.amount;
        if (sum > Global.MaxSize) {
            a.amount = sum - Global.MaxSize;
            b.amount = Global.MaxSize;
        } else {
            a.amount = sum;
            b.amount = 0;
        }
    }

    public static bool areEqual(ItemSlot first, ItemSlot second) {
        if (ReferenceEquals(first,second)) {
            return true;
        }
        if (first == null || second == null) {
            return false;
        }
        if (first.itemObject == null || second.itemObject == null) {
            return false;
        }
        if (first.itemObject.id != second.itemObject.id) {
            return false;
        }
        if (!ItemTagFactory.tagsEqual(first.tags,second.tags)) {
            return false;
        }
        return true;
    }

    public static bool areEqualNoNullCheck(ItemSlot first, ItemSlot second) {
        if (first.itemObject.id != second.itemObject.id) {
            return false;
        }
        if (!ItemTagFactory.tagsEqual(first.tags,second.tags)) {
            return false;
        }
        return true;
    }

    public static bool areEqualWithAmount(ItemSlot first, ItemSlot second) {
        if (!areEqual(first,second)) {
            return false;
        }
        return first.amount == second.amount;
    }

    public static bool canInsert(List<ItemSlot> inventory, ItemSlot toInsert, int maxAmount) {
        if (inventory == null) {
            return false;
        }
        foreach (ItemSlot itemSlot in inventory) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return true;
            }
            if (itemSlot.itemObject.id != toInsert.itemObject.id) {
                return true;
            }
            if (!itemSlot.tags.Equals(toInsert.tags)) {
                return true;
            }
            if (itemSlot.amount + toInsert.amount <= maxAmount) {
                return true;
            }
        }
        return false;
    }

    public static List<ItemSlot> initEmptyInventory(int count) {
        if (count <= 0) {
            return null;
        }
   
        List<ItemSlot> inventory = new List<ItemSlot>();
        for (int i = 0; i < count; i++) {
            inventory.Add(null);
        }
        return inventory; 
    }
 
    public static void insertListIntoInventory(List<ItemSlot> inventory, List<ItemSlot> insertList) {
        if (inventory == null) {
            return;
        }
        int n = 0;
        while (n < insertList.Count) {
            ItemSlot outputItem = insertList[n];

            bool inserted = insertIntoInventory(inventory,outputItem);
            
            if (outputItem.amount == 0 || !inserted) { 
                n ++;
            }
        }
    }

    public static void handleInsert(ItemSlot inputSlot, ItemSlot toInsert) {
        if (inputSlot.itemObject.id != toInsert.itemObject.id) {
            return;
        }
        if (inputSlot.amount >= Global.MaxSize) {
            return;
        }
        // Success
        int sum = inputSlot.amount + toInsert.amount;
        if (sum > Global.MaxSize) {
            toInsert.amount = sum - Global.MaxSize;
            inputSlot.amount = Global.MaxSize;
        } else {
            inputSlot.amount = sum;
            toInsert.amount = 0;
        }
        return;
    }

    public static void sortInventoryByState(List<ItemSlot> inputs,out List<ItemSlot> solidRecipeInputs,out List<ItemSlot> fluidRecipeInputs) {
        solidRecipeInputs = new List<ItemSlot>();
        fluidRecipeInputs = new List<ItemSlot>();
        foreach (ItemSlot itemSlot in inputs) {
            if (itemSlot == null) {
                continue;
            }
            switch (itemSlot.getState()) {
                case ItemState.Solid:
                    solidRecipeInputs.Add(itemSlot);
                    break;
                case ItemState.Fluid:
                    fluidRecipeInputs.Add(itemSlot);
                    break;
            }
        }
    }
}

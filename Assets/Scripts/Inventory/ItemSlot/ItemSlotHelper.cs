using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemSlotHelper
{
    public static void insertIntoInventory(List<ItemSlot> contained, ItemSlot toInsert) {
        for (int i = 0; i < contained.Count; i++) {
            ItemSlot inputSlot = contained[i];
            if (inputSlot == null || inputSlot.itemObject == null) {
                contained[i] = new ItemSlot(toInsert.itemObject,toInsert.amount,toInsert.nbt);
                toInsert.amount=0;
                return;
            }
            if (inputSlot.itemObject.id != toInsert.itemObject.id) {
                continue;
            }
            if (inputSlot.amount >= Global.MaxSize) {
                continue;
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
        for (int n = 0; n < insertList.Count; n++) {
            ItemSlot outputItem = insertList[n];
            insertIntoInventory(inventory,outputItem);
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
}

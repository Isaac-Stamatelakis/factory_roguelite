using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemSlotHelper
{
    public static ItemSlot insert(ItemSlot contained, ItemSlot toInsert) {

        /*
        ItemSlot inputSlot = inputs[i];
        if (inputSlot == null || inputSlot.itemObject == null) {
            inputs[i] = new ItemSlot(itemSlot.itemObject,itemSlot.amount,itemSlot.nbt);
            itemSlot.amount=0;
            return inputs[i];
        }
        if (inputSlot.itemObject.id != itemSlot.itemObject.id) {
            continue;
        }
        if (inputSlot.amount >= Global.MaxSize) {
            continue;
        }
        // Success
        int sum = inputSlot.amount + itemSlot.amount;
        if (sum > Global.MaxSize) {
            itemSlot.amount = sum - Global.MaxSize;
            inputSlot.amount = Global.MaxSize;
        } else {
            inputSlot.amount = sum;
            itemSlot.amount = 0;
        }
        return inputSlot;
        */
        return null;

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

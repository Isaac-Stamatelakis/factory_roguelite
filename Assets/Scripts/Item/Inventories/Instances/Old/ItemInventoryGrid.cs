using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemInventoryGrid : DynamicInventoryGrid
{
    protected override GameObject loadItemAmountNumber(GameObject slot, ItemSlot itemSlot)
    {
        if (itemSlot is null) {
            return null;
        }
        if (itemSlot.itemObject == null) {
            return null;
        }
        return base.loadItemAmountNumber(slot,itemSlot);
    }

    protected override GameObject loadItemImage(GameObject slot, ItemSlot itemSlot)
    {
        if (itemSlot is null) {
            return null;
        }
        if (itemSlot.itemObject == null) {
            return null;
        }
        
        GameObject imageObject = base.loadItemImage(slot, itemSlot);
        return imageObject;
    }
}

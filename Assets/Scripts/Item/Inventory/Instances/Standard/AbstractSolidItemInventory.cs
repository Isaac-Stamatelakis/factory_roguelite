using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ILoadableInventory {
    public void initalize(List<ItemSlot> items);
}
public abstract class AbstractSolidItemInventory : InventoryUI
{
    public override void leftClick(int n)
    {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory " + name + " GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        ItemSlot inventorySlot = inventory[n];
        ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
        if (ItemSlotHelper.areEqual(grabbedSlot,inventorySlot)) {
            // Merge
            int sum = inventorySlot.amount + grabbedSlot.amount;
            if (sum > Global.MaxSize) {
                grabbedSlot.amount = sum-Global.MaxSize;
                inventorySlot.amount = Global.MaxSize;
            } else { // Overflow
                inventorySlot.amount = sum;
                grabbedItemProperties.itemSlot = null;
            }
        } else {    
            // Swap
            inventory[n] = grabbedItemProperties.itemSlot;
            grabbedItemProperties.itemSlot = inventorySlot;
        }
        
        
        unloadItem(n);
        loadItem(n);
        grabbedItemProperties.updateSprite();
    }

    public override void middleClick(int n)
    {
        // TODO 
    }

    public override void rightClick(int n)
    {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory " + name + " GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        ItemSlot inventorySlot = inventory[n];
        ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
        if (inventorySlot == null || inventorySlot.itemObject == null) {
            return;
        }
        if (grabbedItemProperties.setItemSlotFromInventory(inventory,n)) {
            return;
        }
        grabbedItemProperties.addItemSlotFromInventory(inventory,n);
    }
}


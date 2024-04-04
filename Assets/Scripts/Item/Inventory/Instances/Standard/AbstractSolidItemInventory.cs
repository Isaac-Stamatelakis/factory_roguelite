using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public interface ILoadableInventory {
    public void initalize(List<ItemSlot> items);
}

public interface IInventoryListener {
    public void inventoryUpdate();   
}
public abstract class AbstractSolidItemInventory : InventoryUI
{
    private List<IInventoryListener> listeners;

    public void addListener(IInventoryListener listener) {
        if (listeners == null) {
            listeners = new List<IInventoryListener>();
        }
        listeners.Add(listener);
    }
    public override void leftClick(int n)
    {
        SolidItemInventoryHelper.leftClick(inventory,n);
        updateAllListeners();
        unloadItem(n);
        loadItem(n);
        
    }

    private void updateAllListeners() {
        if (listeners == null) {
            return;
        }
        foreach (IInventoryListener listener in listeners) {
            listener.inventoryUpdate();
        }
    }

    public override void middleClick(int n)
    {
        // TODO 
    }

    public override void rightClick(int n)
    {
        SolidItemInventoryHelper.rightClick(inventory,n);
        updateAllListeners();
    }

    
}

public static class SolidItemInventoryHelper {
    public static void rightClick(List<ItemSlot> inventory, int n) {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            return;
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

    public static void leftClick(List<ItemSlot> inventory, int n) {
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory GrabbedItem is null");
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
        grabbedItemProperties.updateSprite();
    }
}
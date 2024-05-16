using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Items.Inventory {
    public interface ILoadableInventory {
        public void initalize(List<ItemSlot> items);
    }

    public interface IInventoryListener {
        public void inventoryUpdate(int n);   
    }

    public abstract class AbstractSolidItemInventory : InventoryUI
    {
        private bool allowInputs = true;
        private List<IInventoryListener> listeners;

        public bool AllowInputs { get => allowInputs; set => allowInputs = value; }

        public void addListener(IInventoryListener listener) {
            if (listeners == null) {
                listeners = new List<IInventoryListener>();
            }
            listeners.Add(listener);
        }
        public override void leftClick(int n)
        {
            SolidItemInventoryHelper.leftClick(inventory,n,allowInputs);
            updateAllListeners(n);
            displayItem(n);
        }

        private void updateAllListeners(int n) {
            if (listeners == null) {
                return;
            }
            foreach (IInventoryListener listener in listeners) {
                listener.inventoryUpdate(n);
            }
        }

        public override void middleClick(int n)
        {
            // TODO 
        }

        public override void rightClick(int n)
        {
            SolidItemInventoryHelper.rightClick(inventory,n);
            updateAllListeners(n);
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
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (inventorySlot == null || inventorySlot.itemObject == null) {
                return;
            }
            
            if (grabbedItemProperties.setItemSlotFromInventory(inventory,n)) {
                return;
            }
            grabbedItemProperties.addItemSlotFromInventory(inventory,n);
        }

        public static void leftClick(List<ItemSlot> inventory, int n, bool allowInputs) {
            GameObject grabbedItem = GameObject.Find("GrabbedItem");
            if (grabbedItem == null) {
                Debug.LogError("Inventory GrabbedItem is null");
            }
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            ItemSlot inventorySlot = inventory[n];
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (!allowInputs && grabbedSlot == null) {
                inventory[n] = null;
                grabbedItemProperties.setItemSlot(inventorySlot);
                return;
            }
            if (ItemSlotHelper.areEqual(grabbedSlot,inventorySlot)) {
                // Merge
                int sum = inventorySlot.amount + grabbedSlot.amount;
                if (sum > Global.MaxSize) {
                    grabbedSlot.amount = sum-Global.MaxSize;
                    inventorySlot.amount = Global.MaxSize;
                } else { // Overflow
                    inventorySlot.amount = sum;
                    grabbedItemProperties.setItemSlot(null);
                }
            } else {    
                // Swap
                inventory[n] = grabbedItemProperties.ItemSlot;
                grabbedItemProperties.setItemSlot(inventorySlot);
            }
        }
    }
}

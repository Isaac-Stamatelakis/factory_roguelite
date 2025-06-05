using System;
using System.Collections.Generic;
using Item.Slot;
using Items.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

namespace Item.Inventory
{
    public class InventoryUIRotator : MonoBehaviour
    {
        private InventoryUI inventoryUI;
        private int counter;
        private int index;
        private int updateTime;
        private List<List<ItemSlot>> inventories;
        private int displaySize;
        public void Initialize(List<List<ItemSlot>> inventories, int displaySize, int fixedUpdatesPerSwitch, bool clear = true, int initialIndex = 0)
        {
            inventoryUI = GetComponent<InventoryUI>();
            if (inventories.Count == 0)
            {
                List<ItemSlot> emptyDisplay = ItemSlotFactory.createEmptyInventory(displaySize);
                inventoryUI.DisplayInventory(emptyDisplay,displaySize,clear);
                return;
            }
            this.inventories = inventories;
            this.displaySize = displaySize;
            this.updateTime = fixedUpdatesPerSwitch;
            index = initialIndex;
            
            counter = 0;
            if (ReferenceEquals(inventoryUI, null))
            {
                Debug.LogError("InventoryUIRotator must be placed on an inventory ui");
                return;
            }
            inventoryUI.DisplayInventory(inventories[index],displaySize,clear);
        }

        public void FixedUpdate()
        {
            if (Keyboard.current.shiftKey.isPressed) return;
            if (ReferenceEquals(inventories, null) || ReferenceEquals(inventoryUI, null)) return;
            counter++;
            if (counter % updateTime != 0) return;
            index++;
            index %= inventories.Count;
            inventoryUI.DisplayInventory(inventories[index], displaySize, false);
            
        }
    }
}

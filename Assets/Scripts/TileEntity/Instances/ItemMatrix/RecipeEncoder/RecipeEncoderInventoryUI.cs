using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Inventory;

namespace TileEntityModule.Instances.Matrix {
    public class RecipeEncoderInventoryUI : InventoryUI
    {
        public void initalize(List<ItemSlot> inventory) {
            this.inventory = inventory;
            initalizeSlots();
        }
        public override void leftClick(int n)
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemContainer.getGrabbedItem();
            if (grabbedItemProperties.itemSlot == null) {
                inventory[n] = null;
            } else {
                inventory[n] = ItemSlotFactory.copy(grabbedItemProperties.itemSlot);
            }
        }

        public override void middleClick(int n)
        {
            
        }

        public override void rightClick(int n)
        {
            ItemSlot selectedSlot = inventory[n];
            if (selectedSlot == null || selectedSlot.itemObject == null) {
                return;
            }
            selectedSlot.amount--;
            if (selectedSlot.amount > 0) {
                return;
            }
            inventory[n] = null;
        }
    }
}


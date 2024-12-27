using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Inventory;
using Items;

namespace TileEntity.Instances.Matrix {
    public class RecipeEncoderInventoryUI : InventoryUI
    {
        public void initalize(List<ItemSlot> inventory) {
            this.inventory = inventory;
            InitalizeSlots();
        }
        public override void leftClick(int n)
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            if (grabbedItemProperties.ItemSlot == null) {
                inventory[n] = null;
            } else {
                inventory[n] = ItemSlotFactory.Copy(grabbedItemProperties.ItemSlot);
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


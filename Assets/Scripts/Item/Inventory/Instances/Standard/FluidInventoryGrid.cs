using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;
using Items.Tags.FluidContainers;

namespace Items.Inventory {
    public class FluidInventoryGrid : InventoryUI, ILoadableInventory
    {
        public void initalize(List<ItemSlot> items) {
            this.inventory = items;
            InitalizeSlots();
        }

        public override void rightClick(int n)
        {
            // Do nothing
        }

        public override void leftClick(int n)
        {
            ItemSlot fluidSlot = inventory[n];
            GameObject grabbedItem = GameObject.Find("GrabbedItem");
            if (grabbedItem == null) {
                Debug.LogError("Inventory " + name + " GrabbedItem is null");
            }
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            FluidContainerHelper.handleClick(grabbedItemProperties,inventory,n);
        }

        public override void middleClick(int n)
        {
            // Do nothing
        }
    }
}


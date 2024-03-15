using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;
using ItemModule.Tags.FluidContainers;


public class FluidInventoryGrid : InventoryUI, ILoadableInventory
{
    public void initalize(List<ItemSlot> items) {
        this.inventory = items;
        initalizeSlots();
    }

    public override void clickSlot(int n)
    {
        ItemSlot fluidSlot = inventory[n];
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory " + name + " GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        FluidContainerHelper.handleClick(grabbedItemProperties,inventory,n);
    }
}

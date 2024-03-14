using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule;


public class FluidInventoryGrid : InventoryUI, ILoadableInventory
{
    public void initalize(List<ItemSlot> items) {
        this.inventory = items;
        initalizeSlots();
    }
    public void FixedUpdate() {
        refreshSlots();
    }

    public override void clickSlot(int n)
    {
        ItemSlot fluidSlot = inventory[n];
        if (fluidSlot == null || fluidSlot.itemObject == null) {
            return;
        }
        GameObject grabbedItem = GameObject.Find("GrabbedItem");
        if (grabbedItem == null) {
            Debug.LogError("Inventory " + name + " GrabbedItem is null");
        }
        GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
        ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
        if (grabbedSlot == null || grabbedSlot.itemObject == null) {
            return;
        }
        if (grabbedSlot.itemObject is not IFluidContainer fluidContainer) {
            return;
        }
        fluidContainer.transferFluid(fluidSlot);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;

namespace ItemModule.Tags.FluidContainers {
    public static class FluidContainerHelper 
    {
        public static void handleClick(GrabbedItemProperties grabbedItemProperties,List<ItemSlot> fluidInventory, int index) {
            ItemSlot container = grabbedItemProperties.itemSlot;
            if (container == null || container.itemObject == null) {
                return;
            }
            if (container.itemObject is not IFluidContainer fluidContainer) {
                return;
            }
            if (container.tags == null) {
                Debug.LogError("FluidContainerHelper method 'handleClick' recieved itemslot container which did not have tags");
                return;
            }
            if (!container.tags.Dict.ContainsKey(ItemTag.FluidContainer)) {
                Debug.LogError("FluidContainerHelper method 'handleClick' recieved itemslot container which did not have tag " + ItemTag.FluidContainer);
            }
            // Input fluid into fluidInventory
            if (fluidInventory[index] == null || fluidInventory[index].itemObject == null) {
                object itemSlotObject = container.tags.Dict[ItemTag.FluidContainer];
                if (itemSlotObject is not ItemSlot itemSlot) {
                    return;
                }
                fluidInventory[index] = itemSlot;
                container.tags.Dict[ItemTag.FluidContainer] = null;
                grabbedItemProperties.updateSprite();
                return;
            }
            // Input fluid into player inventory
            // TODO change way player inventory is gotten
            GameObject player = GameObject.Find("Player");
            PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
            List<ItemSlot> inventory = playerInventory.Inventory;

            if (!ItemSlotHelper.canInsert(inventory,container,fluidContainer.getStorage())) {
                return;
            }
            ItemSlot newItemSlot = ItemSlotFactory.createNewItemSlot(container.itemObject,1);
            newItemSlot.tags.Dict[ItemTag.FluidContainer] = fluidInventory[index];
            fluidInventory[index] = null;
            container.amount -= 1;
            if (container.amount == 0) {
                grabbedItemProperties.itemSlot = newItemSlot;
                container.itemObject = null;
                grabbedItemProperties.updateSprite();
                return;
            }
            ItemSlotHelper.insertIntoInventory(inventory,newItemSlot);
            grabbedItemProperties.updateSprite();
        }
    }
}


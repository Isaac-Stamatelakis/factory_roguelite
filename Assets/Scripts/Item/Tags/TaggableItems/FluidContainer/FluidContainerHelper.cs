using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerModule;

namespace Items.Tags.FluidContainers {
    public static class FluidContainerHelper 
    {
        public static void handleClick(GrabbedItemProperties grabbedItemProperties,List<ItemSlot> fluidInventory, int index) {
            ItemSlot container = grabbedItemProperties.ItemSlot;
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
            // TODO change way player inventory is gotten
            GameObject player = GameObject.Find("Player");
            PlayerInventory playerInventory = player.GetComponent<PlayerInventory>();
            List<ItemSlot> inventory = playerInventory.Inventory;

            // Input fluid into fluidInventory
            if (fluidInventory[index] == null || fluidInventory[index].itemObject == null) {
                object itemSlotObject = container.tags.Dict[ItemTag.FluidContainer];
                if (itemSlotObject is not ItemSlot itemSlot) {
                    return;
                }
                ItemSlot fluidInventorySlot = fluidInventory[index];
                if (ItemSlotHelper.AreEqual(fluidInventorySlot,itemSlot)) { // Merge
                    Debug.Log("Hi");
                }
                fluidInventory[index] = itemSlot;
                container.amount--;
                ItemSlot empty = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
                if (container.amount == 0) {
                    grabbedItemProperties.SetItemSlot(empty);
                } else {
                    if (!ItemSlotHelper.CanInsertIntoInventory(inventory,container,fluidContainer.GetStorage())) {
                    // TODO spawn item
                        return;
                    } else {
                        ItemSlotHelper.InsertIntoInventory(inventory,empty,fluidContainer.GetStorage());
                    }
                }
                return;
            }
            // Input fluid cell into player inventory
            ItemSlot newItemSlot = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
            if (!ItemSlotHelper.CanInsertIntoInventory(inventory,newItemSlot,fluidContainer.GetStorage())) {
                return;
            }
            newItemSlot.tags.Dict[ItemTag.FluidContainer] = fluidInventory[index];
            fluidInventory[index] = null;
            container.amount -= 1;
            if (container.amount == 0) {
                grabbedItemProperties.SetItemSlot(newItemSlot);
                container.itemObject = null;
                return;
            }
            ItemSlotHelper.InsertIntoInventory(inventory,newItemSlot,fluidContainer.GetStorage());
            grabbedItemProperties.UpdateSprite();
        }
    }
}


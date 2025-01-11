using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Tags;
using Items.Tags.FluidContainers;
using PlayerModule;
using UnityEngine;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class FluidItemClickHandler : ItemSlotUIClickHandler
    {
        protected override void LeftClick()
        {
            var inventory = inventoryUI.GetInventory();
            inventoryUI.CallListeners(index);
            ItemSlot container = GrabbedItemProperties.Instance.ItemSlot;
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
            if (inventory[index] == null || inventory[index].itemObject == null) {
                object itemSlotObject = container.tags.Dict[ItemTag.FluidContainer];
                if (itemSlotObject is not ItemSlot itemSlot) {
                    return;
                }
                ItemSlot fluidInventorySlot = inventory[index];
                if (ItemSlotUtils.AreEqual(fluidInventorySlot,itemSlot)) { // Merge
                    Debug.Log("Hi");
                }
                inventory[index] = itemSlot;
                container.amount--;
                ItemSlot empty = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
                if (container.amount == 0) {
                    GrabbedItemProperties.Instance.SetItemSlot(empty);
                } else {
                    if (!ItemSlotUtils.CanInsertIntoInventory(inventory,container,fluidContainer.GetStorage())) {
                    
                        return;
                    } else {
                        ItemSlotUtils.InsertIntoInventory(inventory,empty,fluidContainer.GetStorage());
                    }
                }
                return;
            }
            List<ItemSlot> playerInventory =  PlayerManager.Instance.GetPlayer().PlayerInventory.Inventory;
            ItemSlot newItemSlot = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
            if (!ItemSlotUtils.CanInsertIntoInventory(inventory,newItemSlot,fluidContainer.GetStorage())) {
                return;
            }
            newItemSlot.tags.Dict[ItemTag.FluidContainer] = inventory[index];
            inventory[index] = null;
            container.amount -= 1;
            if (container.amount == 0) {
                GrabbedItemProperties.Instance.SetItemSlot(newItemSlot);
                container.itemObject = null;
                return;
            }
            ItemSlotUtils.InsertIntoInventory(inventory,newItemSlot,fluidContainer.GetStorage());
            GrabbedItemProperties.Instance.UpdateSprite();
        }

        protected override void RightClick()
        {
            
        }

        protected override void MiddleClick()
        {
            
        }

        public override void MiddleMouseScroll()
        {
            
        }
    }
}

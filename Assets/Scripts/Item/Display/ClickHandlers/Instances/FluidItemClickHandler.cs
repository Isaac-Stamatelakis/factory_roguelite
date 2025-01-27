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
            ItemSlot grabbedSlot = GrabbedItemProperties.Instance.ItemSlot;

            if (ItemSlotUtils.IsItemSlotNull(grabbedSlot)) return;

            switch (grabbedSlot.itemObject)
            {
                case FluidTileItem fluidTileItem:
                    InsertFluidTileItem(grabbedSlot,fluidTileItem);
                    break;
                case IFluidContainer fluidContainer:
                    InsertFluidCell(grabbedSlot, fluidContainer);
                    break;
            }
        }

        private void InsertFluidTileItem(ItemSlot itemSlot, FluidTileItem fluidTileItem)
        {
            // This insert option will not be available for players who do not cheat in fluid tiles
            // Inserting can overflow the max fluid limit of containers
            var inventory = inventoryUI.GetInventory();
            
            ItemSlot inventorySlot = inventory[index];
            const int UNIT_RATIO = 1000; // Unit change from solid to mL
            if (ItemSlotUtils.IsItemSlotNull(inventorySlot))
            {
                itemSlot.amount *= UNIT_RATIO;
                inventory[index] = itemSlot;
            } else if (inventorySlot.itemObject.id != itemSlot.itemObject.id)
            {
                inventory[index].amount = itemSlot.amount * UNIT_RATIO;
            } else
            {
                return;
            }
            
            GrabbedItemProperties.Instance.SetItemSlot(null);
            GrabbedItemProperties.Instance.UpdateSprite();
            inventoryUI.CallListeners(index);
            inventoryUI.RefreshSlots();
 
        }

        private void InsertFluidCell(ItemSlot container, IFluidContainer fluidContainer)
        {
            var inventory = inventoryUI.GetInventory();
            inventoryUI.CallListeners(index);
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

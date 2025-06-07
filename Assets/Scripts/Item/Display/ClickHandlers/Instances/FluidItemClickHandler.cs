using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Tags;
using Items.Tags.FluidContainers;
using Player;
using PlayerModule;
using UnityEngine;
using UnityEngine.InputSystem;

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
                case IFluidContainerData fluidContainer:
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

        private void InsertFluidCell(ItemSlot container, IFluidContainerData iFluidContainerData)
        {
            if (container.tags?.Dict == null || !container.tags.Dict.ContainsKey(ItemTag.FluidContainer) ||
                container.tags.Dict[ItemTag.FluidContainer] == null)
            {
                ExtractFromInventoryIntoFluidCell(container,iFluidContainerData, PlayerManager.Instance.GetPlayer().PlayerInventory);
        
            } else {
                InsertFluidCellIntoInventory(container, iFluidContainerData);
            }
            inventoryUI.CallListeners(index);
            inventoryUI.RefreshSlots();
            GrabbedItemProperties.Instance.UpdateSprite();
        }

        private void ExtractFromInventoryIntoFluidCell(ItemSlot container, IFluidContainerData containerData, PlayerInventory playerInventory)
        {
            ItemSlotUtils.BuildTagDictIfNull(container);
            container.tags.Dict.TryAdd(ItemTag.FluidContainer, null);
            ItemSlot inventorySlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(inventorySlot)) return;
            
            ItemSlot tagFluidSlot = container.tags.Dict[ItemTag.FluidContainer] as ItemSlot;
            
            if (ItemSlotUtils.IsItemSlotNull(tagFluidSlot))
            {
                ItemSlot fluidItem = CreateNewFluidItem(inventorySlot, containerData.GetStorage());
                GivePlayerFluidItem(fluidItem, container, playerInventory);
                return;
            }
            
            if (tagFluidSlot.amount >= containerData.GetStorage() || !ItemSlotUtils.AreEqual(tagFluidSlot, inventorySlot)) return;
            uint space = containerData.GetStorage() - tagFluidSlot.amount;

            uint toTake = inventorySlot.amount > space ? space : inventorySlot.amount;
            inventorySlot.amount -= toTake;
            ItemSlot mergedFluidItem = new ItemSlot(inventorySlot.itemObject, tagFluidSlot.amount + toTake, null);
            GivePlayerFluidItem(mergedFluidItem, container, playerInventory);

        }

        private void GivePlayerFluidItem(ItemSlot fluidItem, ItemSlot container, PlayerInventory playerInventory)
        {
            if (container.amount == 1)
            {
                container.tags.Dict[ItemTag.FluidContainer] = fluidItem;
                GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
                grabbedItemProperties.SetItemSlot(null); // Set to null first so tag display is relaoded
                grabbedItemProperties.SetItemSlot(container);
                return;
            }
           
            ItemTagCollection itemTagCollection = new ItemTagCollection(new Dictionary<ItemTag, object>
            {
                { ItemTag.FluidContainer, fluidItem }
            });
            ItemSlot newCell = new ItemSlot(container.itemObject, 1, itemTagCollection);
            container.amount--;
            playerInventory.Give(newCell);
            inventoryUI.Connection?.RefreshSlots();
            
        }

        private ItemSlot CreateNewFluidItem(ItemSlot inventorySlot, uint maxStorage)
        {
            uint size = GlobalHelper.Clamp(inventorySlot.amount, 0, maxStorage);
            ItemSlot newSlot = new ItemSlot(inventorySlot.itemObject, size,null);
            inventorySlot.amount -= size;
            return newSlot;
        }
        private void InsertFluidCellIntoInventory(ItemSlot container, IFluidContainerData containerData)
        {
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            bool holdingShift = Keyboard.current.shiftKey.isPressed;
            
            var inventory = inventoryUI.GetInventory();
            object itemSlotObject = container.tags.Dict[ItemTag.FluidContainer];
            if (itemSlotObject is not ItemSlot fluidContainerFluidSlot || ItemSlotUtils.IsItemSlotNull(fluidContainerFluidSlot)) {
                return;
            }
            ItemSlot fluidInventorySlot = inventory[index];
            
            if (string.Equals(fluidInventorySlot?.itemObject?.id,fluidContainerFluidSlot?.itemObject?.id)) { // No need to check tags
                // Merge
                InsertIntoInventory(holdingShift);
                return;
            }
            inventory[index] = fluidContainerFluidSlot;
            container.amount--;
            ItemSlot empty = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
            playerScript.PlayerInventory.Give(empty);
            return;
            
            void InsertIntoInventory(bool all)
            {
                uint maxSize = inventoryUI.MaxSize;
                if (fluidInventorySlot.amount >= maxSize) return;
                while (container.amount > 0 && fluidContainerFluidSlot.amount > 0)
                {
                    fluidInventorySlot.amount += fluidContainerFluidSlot.amount;
                    container.amount--;
                    if (fluidInventorySlot.amount > maxSize)
                    {
                        ItemSlot newContainer = new  ItemSlot(container.itemObject, 1, null);
                        ItemSlot splitFluidItem = new ItemSlot(fluidContainerFluidSlot.itemObject, fluidContainerFluidSlot.amount - maxSize, null);
                        ItemSlotUtils.AddTag(newContainer, ItemTag.FluidContainer,splitFluidItem);
                        fluidInventorySlot.amount = maxSize;
                        playerScript.PlayerInventory.Give(newContainer);
                        break;
                    }
                    ItemSlot newEmpty = ItemSlotFactory.CreateNewItemSlot(container.itemObject,1);
                    playerScript.PlayerInventory.Give(newEmpty);
                    if (!all) break;
                }
            }
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

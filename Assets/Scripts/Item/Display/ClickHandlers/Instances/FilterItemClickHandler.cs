using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using UnityEngine;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class FilterItemClickHandler : ItemSlotUIClickHandler
    {
        protected override void LeftClick()
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            var inventory = inventoryUI.GetInventory();
            if (ItemSlotUtils.IsItemSlotNull(grabbedSlot))
            {
                inventory[index] = null;
            }
            else
            {
                inventory[index] = new ItemSlot(grabbedSlot.itemObject, 1, null);
            }
            inventoryUI.RefreshSlots();
            inventoryUI.CallListeners(index);
        }

        protected override void MiddleClick()
        {
            
        }

        public override void MiddleMouseScroll()
        {
            
        }

        protected override void RightClick()
        {
            var inventory = inventoryUI.GetInventory();
            inventory[index] = null;
            inventoryUI.RefreshSlots();
            inventoryUI.CallListeners(index);
        }
    }
}

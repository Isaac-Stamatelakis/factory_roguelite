using Item.Slot;
using Items;
using UnityEngine;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class EncodedRecipeClickHandler : ItemSlotUIClickHandler
    {
        protected override void LeftClick()
        {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            var inventory = inventoryUI.GetInventory();
            if (grabbedItemProperties.ItemSlot == null) {
                inventory[index] = null;
            } else {
                inventory[index] = ItemSlotFactory.Copy(grabbedItemProperties.ItemSlot);
            }
        }

        protected override void MiddleClick()
        {
            
        }

        protected override void RightClick()
        {
            ItemSlot selectedSlot = inventoryUI.GetItemSlot(index);
            if (selectedSlot == null || selectedSlot.itemObject == null) {
                return;
            }
            selectedSlot.amount--;
            if (selectedSlot.amount > 0) {
                return;
            }
            var inventory = inventoryUI.GetInventory();
            inventory[index] = null;
        }
    }
}

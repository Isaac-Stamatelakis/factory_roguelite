using Item.Slot;
using Recipe.Viewer;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class RecipeItemClickHandler : ItemSlotUIClickHandler
    {
        protected override void LeftClick()
        {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (itemSlot == null) {
                return;
            }
            RecipeViewerHelper.DisplayCraftingOfItem(itemSlot);
        }

        protected override void MiddleClick()
        {
            // Does nothing
        }

        protected override void RightClick()
        {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (itemSlot == null) {
                return;
            }
            RecipeViewerHelper.DisplayUsesOfItem(itemSlot);
        }

    
    }
}

using Item.Display.ClickHandlers;
using Item.Slot;
using Recipe.Viewer;
using UI.Catalogue.InfoViewer;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class RecipeItemClickHandler : ItemSlotUIClickHandler
    {
        protected override void LeftClick()
        {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                return;
            }
            CatalogueInfoUtils.DisplayItemInformation(itemSlot);
        }

        protected override void MiddleClick()
        {
            // Does nothing
        }

        public override void MiddleMouseScroll()
        {
            
        }

        protected override void RightClick()
        {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) {
                return;
            }
            CatalogueInfoUtils.DisplayItemUses(itemSlot);
        }

    
    }
}

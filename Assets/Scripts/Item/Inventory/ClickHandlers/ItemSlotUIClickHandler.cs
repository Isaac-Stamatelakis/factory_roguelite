using System;
using Item.Slot;
using Items.Inventory;
using Recipe.Viewer;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Item.Inventory
{
    public abstract class ItemSlotUIClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public bool EnableToolTip;
        protected InventoryUI inventoryUI;
        protected int index;
        public void Initialize(InventoryUI parent, int index) {
            this.inventoryUI = parent;
            this.index = index;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    LeftClick();
                    break;
                case PointerEventData.InputButton.Right:
                    RightClick();
                    break;
                case PointerEventData.InputButton.Middle:
                    MiddleClick();
                    break;
                default:
                    break;
            }
        }
    
        public void ShowRecipes() {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            RecipeViewerHelper.DisplayCraftingOfItem(itemSlot);
        }
        public void ShowUses() {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;
            }
            RecipeViewerHelper.DisplayUsesOfItem(itemSlot);
        }
        protected abstract void LeftClick();
        protected abstract void RightClick();
        protected abstract void MiddleClick();
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (!EnableToolTip || ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            ToolTipController.Instance.ShowToolTip(transform.position,itemSlot.itemObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!EnableToolTip) return; 
            ToolTipController.Instance.HideToolTip();
        }
    }
}

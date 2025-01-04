using System;
using Item.Slot;
using Items.Inventory;
using UI.Catalogue.InfoViewer;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Item.Display.ClickHandlers
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
            switch (inventoryUI.InventoryInteractMode)
            {
                case InventoryInteractMode.Standard:
                    StandardClick(eventData);
                    break;
                case InventoryInteractMode.Recipe:
                    RecipeModeClick(eventData);
                    break;
                case InventoryInteractMode.UnInteractable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StandardClick(PointerEventData eventData)
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

        private void RecipeModeClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    ShowRecipes();
                    break;
                case PointerEventData.InputButton.Right:
                    ShowUses();
                    break;
                default:
                    break;
            }
        }
    
        public void ShowRecipes() {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            InfoViewUtils.DisplayItemInformation(itemSlot);
        }
        public void ShowUses() {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            InfoViewUtils.DisplayItemUses(itemSlot);
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

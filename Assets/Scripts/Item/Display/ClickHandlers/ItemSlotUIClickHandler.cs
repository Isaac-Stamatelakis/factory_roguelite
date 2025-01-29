using System;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using UI.Catalogue.InfoViewer;
using UI.ToolTip;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Item.Display.ClickHandlers
{
    public abstract class ItemSlotUIClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        protected InventoryUI inventoryUI;
        public InventoryUI InventoryUI => inventoryUI;
        protected int index;
        public int Index => index;
        public ItemSlotUI ItemSlotUI;
        public void Initialize(InventoryUI parent, int index) {
            this.inventoryUI = parent;
            this.index = index;
            ItemSlotUI = GetComponent<ItemSlotUI>();
        }
        
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (ReferenceEquals(inventoryUI, null)) return;
            switch (inventoryUI.InventoryInteractMode)
            {
                case InventoryInteractMode.Standard:
                case InventoryInteractMode.BlockInput:
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
            if (!inventoryUI.ValidateItemSlot(inventoryUI.GetItemSlot(index)) && !inventoryUI.ValidateItemSlot(GrabbedItemProperties.Instance.ItemSlot)) return;
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
            CatalogueInfoUtils.DisplayItemInformation(itemSlot);
        }
        public void ShowUses() {
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            CatalogueInfoUtils.DisplayItemUses(itemSlot);
        }

        public ItemSlot GetInventoryItem()
        {
            return inventoryUI?.GetItemSlot(index);
        }

        public void SetInventoryItem(ItemSlot itemSlot)
        {
            inventoryUI?.SetItem(index,itemSlot);
        }
        protected abstract void LeftClick();
        protected abstract void RightClick();
        protected abstract void MiddleClick();
        public abstract void MiddleMouseScroll();
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ReferenceEquals(inventoryUI, null)) return;
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (!inventoryUI.EnableToolTip || ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            ToolTipController.Instance.ShowToolTip(transform.position,itemSlot.itemObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (inventoryUI is not { EnableToolTip: true }) return; 
            ToolTipController.Instance.HideToolTip();
        }
    }
}
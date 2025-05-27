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
        [NonSerialized] public ItemSlotUI ItemSlotUI;
        private bool focused = false;
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
                case InventoryInteractMode.OverrideAction:
                    var action = inventoryUI.GetOverrideAction();
                    action?.Invoke(eventData.button, index);
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

        public void FixedUpdate()
        {
            if (focused)
            {
                DisplayTooltip();
            }
        }

        private void DisplayTooltip()
        {
            if (ReferenceEquals(inventoryUI, null) || !inventoryUI.EnableToolTip) return;
            if (inventoryUI.ToolTipOverride != null)
            {
                string overrideToolTip = inventoryUI.ToolTipOverride.Invoke(index);
                ToolTipController.Instance.ShowToolTip(transform.position,overrideToolTip);
                return;
            }
            ItemSlot itemSlot = inventoryUI.GetItemSlot(index);
            if (ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                ToolTipController.Instance.HideToolTip();
                return;
            }
            ToolTipController.Instance.ShowToolTip(transform.position,itemSlot);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            focused = true;
            DisplayTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            focused = false;
            if (inventoryUI is not { EnableToolTip: true }) return; 
            ToolTipController.Instance.HideToolTip();
        }
    }
}
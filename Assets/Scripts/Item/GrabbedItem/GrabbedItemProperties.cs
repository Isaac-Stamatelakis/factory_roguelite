using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using PlayerModule.KeyPress;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Item.GrabbedItem {
    public class GrabbedItemProperties : MonoBehaviour
    {
        private static GrabbedItemProperties instance;
        public void Awake() {
            instance = this;
            uiCamera = GetComponentInParent<Canvas>().worldCamera;
            rectTransform = GetComponent<RectTransform>();
        }
        
        public ItemSlotUI ItemSlotUI;
        public ItemSlot ItemSlot {get => itemSlot;}
        public static GrabbedItemProperties Instance { get => instance;}
        private ItemSlot itemSlot;
        private bool mouseDown = false;
        private bool listenMouse = false;
        private ItemSlotDoubleClickEvent doubleClickEvent;
        private ItemSlotUIDragEvent dragEvent;
        private ItemSlotUIClickHandler takeRightClickSlot;
        private Camera uiCamera;
        private RectTransform rectTransform;

        public bool DragEventActive => dragEvent != null;
        
        
        void Update()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                Mouse.current.position.ReadValue(),
                uiCamera,
                out Vector2 localPos
            );
            rectTransform.anchoredPosition = localPos;
            bool leftClick = Mouse.current.leftButton.wasPressedThisFrame;
            if (leftClick && EventSystem.current.IsPointerOverGameObject())
            {
                if (!ItemSlotUtils.IsItemSlotNull(itemSlot))
                {
                    listenMouse = true;
                    dragEvent = new ItemSlotUIDragEvent(itemSlot);
                }
                TryAddDoubleClick();
            }
            if (doubleClickEvent != null)
            {
                doubleClickEvent.Tick();
                if (doubleClickEvent.Expired()) doubleClickEvent = null;
            }
        }
        

        private void FixedUpdate()
        {
            mouseDown = listenMouse && Mouse.current.leftButton.isPressed;
            if (mouseDown)
            {
                SolidItemClickHandler clickHandler = PlayerKeyPress.GetPointerOverComponent<SolidItemClickHandler>();
                dragEvent?.DragNewSlot(clickHandler);
            }
            else
            {
                listenMouse = false;
                if (dragEvent != null)
                {
                    if (dragEvent.Release())
                    {
                        doubleClickEvent = new ItemSlotDoubleClickEvent(PlayerKeyPress.GetPointerOverComponent<SolidItemClickHandler>());
                    }
                    dragEvent = null;
                }
                
            }
        }

        public void SetDeIterateSlot(ItemSlotUIClickHandler itemSlotUIClickHandler)
        {
            takeRightClickSlot = itemSlotUIClickHandler;
        }

        public bool HaveTakenFromSlot(ItemSlotUIClickHandler itemSlotUIClickHandler)
        {
            return !ReferenceEquals(takeRightClickSlot, null) && takeRightClickSlot.Equals(itemSlotUIClickHandler);
        }

        private void TryAddDoubleClick()
        {
            SolidItemClickHandler clickHandler = PlayerKeyPress.GetPointerOverComponent<SolidItemClickHandler>();
            if (ReferenceEquals(clickHandler, null)) return;
            if (doubleClickEvent != null && clickHandler.Equals(doubleClickEvent.ClickHandler))
            {
                List<ItemSlot> inventory = clickHandler.InventoryUI.GetInventory();
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (i == clickHandler.Index) continue;
                    if (!ItemSlotUtils.AreEqual(inventory[i],itemSlot)) continue;
                    ItemSlotUtils.InsertIntoSlot(itemSlot,inventory[i],Global.MAX_SIZE);
                    if (itemSlot.amount < Global.MAX_SIZE) continue;
                    doubleClickEvent = null;
                    return;
                }
                doubleClickEvent = null;
                return;
            }
            ItemSlot clickedSlot = clickHandler.GetInventoryItem();
            if (ItemSlotUtils.IsItemSlotNull(clickedSlot)) return;
            doubleClickEvent = new ItemSlotDoubleClickEvent(clickHandler);
        }

        public void TerminateDragEvent()
        {
            dragEvent = null;
        }

        public void SetItemSlot(ItemSlot itemSlot) {
            this.itemSlot = itemSlot;
            UpdateSprite();
        }

        public void UpdateSprite() {
            ItemSlotUI.Display(itemSlot);
        }

        public void DisplayTemp(ItemSlot tempSlot)
        {
            ItemSlotUI.Display(tempSlot);
        }
        
        public bool SetItemSlotFromInventory(List<ItemSlot> inventory, int n)
        {
            if (ItemSlotUtils.IsItemSlotNull(inventory[n])) return false;
            if (!ItemSlotUtils.IsItemSlotNull(itemSlot)) return false;
            ItemSlot newSlot = ItemSlotFactory.CreateNewItemSlot(inventory[n].itemObject,1);
            inventory[n].amount--;
            SetItemSlot(newSlot);
            return true;
        }

        public void AddItemSlotFromInventory(List<ItemSlot> inventory, int n) {
            ItemSlot inventorySlot = inventory[n];
            if (!ItemSlotUtils.AreEqual(ItemSlot,inventorySlot)) {
                return;
            }
            if (inventorySlot.amount >= Global.MAX_SIZE) {
                return;
            }
            inventorySlot.amount++;
            this.ItemSlot.amount--;
            UpdateSprite();
        }
    }
}


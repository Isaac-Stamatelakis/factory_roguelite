using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Item.Display.ClickHandlers;
using Item.Slot;
using Items.Inventory;
using PlayerModule.KeyPress;

namespace Items {
    public class GrabbedItemProperties : MonoBehaviour
    {
        private static GrabbedItemProperties instance;
        public void Awake() {
            instance = this;
        }
        
        public ItemSlotUI ItemSlotUI;
        public ItemSlot ItemSlot {get => itemSlot;}
        public static GrabbedItemProperties Instance { get => instance;}
        private ItemSlot itemSlot;
        private bool mouseDown = false;
        private bool listenMouse = false;
        
        private ItemSlotUIDragEvent dragEvent;
        
        void Update()
        {
            Vector2 position = Input.mousePosition;
            transform.position = position;
            if (Input.GetMouseButtonDown(0) && !ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                listenMouse = true;
                dragEvent = new ItemSlotUIDragEvent(itemSlot);
            }
            
        }

        private void FixedUpdate()
        {
            mouseDown = listenMouse && Input.GetMouseButton(0);
            if (mouseDown)
            {
                ItemSlotUIClickHandler clickHandler = PlayerKeyPress.GetPointerOverComponent<ItemSlotUIClickHandler>();
                dragEvent.DragNewSlot(clickHandler);
            }
            else
            {
                listenMouse = false;
                if (dragEvent != null)
                {
                    dragEvent.Release();
                    dragEvent = null;
                }
                
            }
        }

        public void SetItemSlot(ItemSlot itemSlot) {
            this.itemSlot = itemSlot;
            UpdateSprite();
        }

        public void UpdateSprite() {
            ItemSlotUI.Display(itemSlot);
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
            if (ItemSlot.amount >= Global.MaxSize) {
                return;
            }
            inventorySlot.amount--;
            this.ItemSlot.amount += 1;
            UpdateSprite();
        }
    }

    public class ItemSlotUIDragEvent
    {
        public ItemSlotUIDragEvent(ItemSlot grabbedSlot)
        {
            totalSum = grabbedSlot.amount;
            this.grabbedSlot = grabbedSlot;
        }

        private HashSet<ItemSlotUIClickHandler> draggedItems = new HashSet<ItemSlotUIClickHandler>();
        private HashSet<ItemSlotUIClickHandler> toSplit = new HashSet<ItemSlotUIClickHandler>();
        private ItemSlot grabbedSlot;
        private uint totalSum;
        private ItemSlotUIClickHandler lastDragged;
        
        public void DragNewSlot(ItemSlotUIClickHandler clickHandler)
        {
            if (ReferenceEquals(clickHandler, null)) return;
            lastDragged = clickHandler;
            ItemSlot clickHandlerSlot = clickHandler.GetInventoryItem();
            bool slotNull = ItemSlotUtils.IsItemSlotNull(clickHandlerSlot);
            if (!slotNull && !ItemSlotUtils.AreEqual(clickHandlerSlot,grabbedSlot)) return;
            if (!draggedItems.Add(clickHandler) || draggedItems.Count == 0) return;
            
            if (!slotNull) totalSum += clickHandlerSlot.amount;
            uint amount = totalSum / (uint)(toSplit.Count+1);
            if (amount == 0) return; // Adding another to split will cause amount to be zero so don't add
            toSplit.Add(clickHandler);
            ItemSlot temp = new ItemSlot(grabbedSlot.itemObject, amount, grabbedSlot.tags);
            foreach (ItemSlotUIClickHandler draggedSlot in toSplit)
            {
                draggedSlot.ItemSlotUI.Display(temp);
            }
            
        }

        public void Reset()
        {
            foreach (ItemSlotUIClickHandler draggedSlot in toSplit)
            {
                ItemSlot clickHandlerSlot = draggedSlot.GetInventoryItem();
                draggedSlot.ItemSlotUI.Display(clickHandlerSlot);
            }
        }

        public void Split()
        {
            uint amount = totalSum / (uint)toSplit.Count;
            foreach (ItemSlotUIClickHandler draggedSlot in toSplit)
            {
                ItemSlot newSlot = new ItemSlot(grabbedSlot.itemObject, amount, grabbedSlot.tags);
                draggedSlot.SetInventoryItem(newSlot);
            }
            grabbedSlot.amount -= GetAmountFromGrabbed();
        }

        private uint GetAmountFromGrabbed()
        {
            return (grabbedSlot.amount / (uint) toSplit.Count) * (uint)toSplit.Count; 
        }

        public void Release()
        {
            if (toSplit.Count < 2) return;
            ItemSlot lastDraggedSlot = lastDragged.GetInventoryItem();
            if (ItemSlotUtils.IsItemSlotNull(lastDraggedSlot) || ItemSlotUtils.AreEqual(lastDraggedSlot, grabbedSlot))
            {
                Split();
            }
            else
            {
                Reset();
            }
        }
    }
}


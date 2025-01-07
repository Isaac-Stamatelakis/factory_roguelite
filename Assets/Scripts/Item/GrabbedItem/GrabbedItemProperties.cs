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
        private ItemSlotDoubleClickEvent doubleClickEvent;
        private ItemSlotUIDragEvent dragEvent;
        
        void Update()
        {
            Vector2 position = Input.mousePosition;
            transform.position = position;
            bool mouseClick = Input.GetMouseButtonDown(0);
            if (mouseClick && !ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                listenMouse = true;
                dragEvent = new ItemSlotUIDragEvent(itemSlot);
            }

            if (mouseClick)
            {
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
                    if (dragEvent.Release())
                    {
                        doubleClickEvent = new ItemSlotDoubleClickEvent(PlayerKeyPress.GetPointerOverComponent<ItemSlotUIClickHandler>());
                    }
                    dragEvent = null;
                }
                
            }
        }

        private void TryAddDoubleClick()
        {
            ItemSlotUIClickHandler clickHandler = PlayerKeyPress.GetPointerOverComponent<ItemSlotUIClickHandler>();
            if (ReferenceEquals(clickHandler, null)) return;
            if (doubleClickEvent != null && clickHandler.Equals(doubleClickEvent.ClickHandler))
            {
                List<ItemSlot> inventory = clickHandler.InventoryUI.GetInventory();
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (i == clickHandler.Index) continue;
                    if (!ItemSlotUtils.AreEqual(inventory[i],itemSlot)) continue;
                    ItemSlotUtils.InsertIntoSlot(itemSlot,inventory[i],Global.MaxSize);
                    if (itemSlot.amount < Global.MaxSize) continue;
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
            if (inventorySlot.amount >= Global.MaxSize) {
                return;
            }
            inventorySlot.amount++;
            this.ItemSlot.amount--;
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
            GrabbedItemProperties.Instance.DisplayTemp(temp);
            
        }

        public void Reset()
        {
            foreach (ItemSlotUIClickHandler draggedSlot in toSplit)
            {
                ItemSlot clickHandlerSlot = draggedSlot.GetInventoryItem();
                draggedSlot.ItemSlotUI.Display(clickHandlerSlot);
            }
            GrabbedItemProperties.Instance.UpdateSprite();
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
            ItemSlotUtils.InsertIntoSlot(lastDragged.GetInventoryItem(),grabbedSlot,Global.MaxSize);
        }

        private uint GetAmountFromGrabbed()
        {
            return (grabbedSlot.amount / (uint) toSplit.Count) * (uint)toSplit.Count; 
        }

        public bool Release()
        {
            if (toSplit.Count < 2) return false;
            ItemSlot lastDraggedSlot = lastDragged.GetInventoryItem();
            bool split = ItemSlotUtils.IsItemSlotNull(lastDraggedSlot) || ItemSlotUtils.AreEqual(lastDraggedSlot, grabbedSlot);
            if (split)
            {
                Split();
            }
            else
            {
                Reset();
            }
            GrabbedItemProperties.Instance.UpdateSprite();
            return !split;
        }
    }

    public class ItemSlotDoubleClickEvent
    {
        public ItemSlotUIClickHandler ClickHandler;
        private ItemSlot doubleClickSlot;
        private static readonly float lifeTime = 0.33f;
        private float time;
        public ItemSlotDoubleClickEvent(ItemSlotUIClickHandler clickHandler)
        {
            this.ClickHandler = clickHandler;
        }

        public void Tick()
        {
            time += Time.deltaTime;
        }
        

        public bool Expired()
        {
            return time > lifeTime;
        }
    }
}


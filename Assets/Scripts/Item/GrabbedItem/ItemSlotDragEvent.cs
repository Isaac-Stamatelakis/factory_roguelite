using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using PlayerModule.KeyPress;

namespace Item.GrabbedItem
{
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
            if (!clickHandler.InventoryUI.ValidateInput(grabbedSlot)) return;
            lastDragged = clickHandler;
            ItemSlot clickHandlerSlot = clickHandler.GetInventoryItem();
            bool slotNull = ItemSlotUtils.IsItemSlotNull(clickHandlerSlot);
            if (!slotNull && !ItemSlotUtils.AreEqual(clickHandlerSlot,grabbedSlot)) return;
            if (!draggedItems.Add(clickHandler) || draggedItems.Count == 0) return;
            
            if (!slotNull) totalSum += clickHandlerSlot.amount;
            uint amount = totalSum / (uint)(toSplit.Count+1);
            if (amount == 0) return; // Adding another to split will cause amount to be zero so don't add
            
            toSplit.Add(clickHandler);
            if (toSplit.Count < 2) return;
            
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
}
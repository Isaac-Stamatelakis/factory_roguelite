using System;
using System.Collections;
using System.Collections.Generic;
using Item.Inventory;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

namespace Items.Inventory {
    public interface IInventoryListener
    {
        public void InventoryUpdate(int n);
    }
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private ItemSlotUI itemSlotUIPrefab;
        protected List<ItemSlotUI> slots = new List<ItemSlotUI>();
        protected List<ItemSlot> inventory;
        protected List<IInventoryListener> listeners = new List<IInventoryListener>();
        private int highlightedSlot = -1;
        public List<ItemSlot> Inventory => inventory;
        
        public void DisplayInventory(List<ItemSlot> displayInventory)
        {
            if (displayInventory == null)
            {
                GlobalHelper.deleteAllChildren(transform);
                return;
            }
            DisplayInventory(displayInventory, displayInventory.Count);
        }

        public void DisplayInventory(List<ItemSlot> displayInventory, int displayAmount)
        {
            inventory = displayInventory;
            while (slots.Count < displayAmount)
            {
                AddSlot();
            }
            while (slots.Count > displayAmount)
            {
                PopSlot();
            }

            for (int i = 0; i < displayAmount; i++)
            {
                slots[i].Display(displayInventory[i]);
            }
        }
        
        
        public void AddListener(IInventoryListener listener) {
            listeners.Add(listener);
        }

        public void CallListeners(int index)
        {
            foreach (IInventoryListener listener in listeners)
            {
                listener.InventoryUpdate(index);
            }
        }
        
        protected void AddSlot() {
            ItemSlotUI slot = Instantiate(itemSlotUIPrefab, transform);
            slots.Add(slot);
            InitClickHandler(slot.transform,slots.Count-1);
        }

        protected void PopSlot() {
            if (slots.Count <= 0) {
                return;
            }
            ItemSlotUI slot = slots[^1]; // last
            slots.RemoveAt(slots.Count-1);
            Destroy(slot.gameObject);
        }
        
        public ItemSlot GetItemSlot(int index) {
            if (index < 0 || index >= inventory.Count) {
                return null;
            }
            return inventory[index];
        }
        
        public void RefreshSlots() {
            if (slots == null || inventory == null) {
                return;
            }
            for (int n = 0; n < slots.Count; n ++) {
                DisplayItem(n);
            }
        }
        public void DisplayItem(int n) {
            if (n < 0 || n >= slots.Count || n >= inventory.Count) {
                return;
            }
            slots[n].Display(inventory[n]);
        }
        
        protected void InitClickHandler(Transform slot, int n) {
            ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
            if (clickHandler != null) {
                clickHandler.Initialize(this,n);
            }
        }
        
        public virtual void SetItem(int n, ItemSlot data) {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            inventory[n]=data;
            DisplayItem(n);
            CallListeners(n);
        }

        public void IterateSlot(int n, uint amount)
        {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            inventory[n].amount += amount;
            DisplayItem(n);
            CallListeners(n);
        }

        public void HighlightSlot(int n)
        {
            if (n == highlightedSlot) {
                return;
            }
            
            if (highlightedSlot >= 0)
            {
                slots[highlightedSlot].GetComponent<Image>().color = itemSlotUIPrefab.GetComponent<Image>().color;
            }
            
            highlightedSlot = n;
            slots[highlightedSlot].GetComponent<Image>().color = new Color(255/255f,215/255f,0,100/255f);
        }
    }

}


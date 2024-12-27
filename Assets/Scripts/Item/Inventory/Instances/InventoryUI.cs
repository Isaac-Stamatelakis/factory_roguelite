using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

namespace Items.Inventory {
    public interface IItemUIClickReciever {
        public void leftClick(int n);
        public void rightClick(int n);
        public void middleClick(int n);
    }
    public abstract class InventoryUI : MonoBehaviour, IItemUIClickReciever {
        protected List<ItemSlotUI> slots = new List<ItemSlotUI>();
        protected List<ItemSlot> inventory;
        public ItemSlot GetItemSlot(int index) {
            if (index < 0 || index >= inventory.Count) {
                return null;
            }
            return inventory[index];
        }
        protected void InitalizeSlots() {
            for (int n = 0; n < inventory.Count; n ++) {
                InitSlot(n);
                DisplayItem(n);
            }
        }

        public void FixedUpdate() {
            //RefreshSlots();
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
        protected virtual void InitSlot(int n) {
            Transform slotTransform = transform.Find("slot" + n);
            if (slotTransform == null) {
                Debug.LogError("slot" + n + " doesn't exist but tried to load it into  inventory " + name);
                slots.Add(null);
                return;
            }
            slots.Add(slotTransform.GetComponent<ItemSlotUI>());
            InitClickHandler(slotTransform,n);
        }

        protected void InitClickHandler(Transform slot, int n) {
            
            ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
            if (clickHandler == null) {
                clickHandler = slot.gameObject.AddComponent<ItemSlotUIClickHandler>();
            }
            clickHandler.init(this,n);
        }
        
        public virtual void SetItem(int n, ItemSlot data) {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            inventory[n]=data;
            DisplayItem(n);
        }
    

        public abstract void rightClick(int n);
        public abstract void leftClick(int n);
        public abstract void middleClick(int n);
    }

}


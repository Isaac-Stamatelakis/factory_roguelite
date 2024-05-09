using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ItemModule;

namespace ItemModule.Inventory {
    public interface IItemUIClickReciever {
        public void leftClick(int n);
        public void rightClick(int n);
        public void middleClick(int n);
    }
    public abstract class InventoryUI : MonoBehaviour, IItemUIClickReciever {
        [SerializeField] protected GameObject slotPrefab;
        protected List<GameObject> slots = new List<GameObject>();
        protected List<ItemSlot> inventory;
        public ItemSlot getItemSlot(int index) {
            if (index < 0 || index >= inventory.Count) {
                return null;
            }
            return inventory[index];
        }
        protected void initalizeSlots() {
            for (int n = 0; n < inventory.Count; n ++) {
                initSlot(n);
                loadItem(n);
            }
        }

        public void FixedUpdate() {
            refreshSlots();
        }

        protected void refreshSlots() {
            if (slots == null || inventory == null) {
                return;
            }
            for (int n = 0; n < slots.Count; n ++) {
                GameObject slot = slots[n];
                if (inventory[n] == null || inventory[n].itemObject == null) {
                    ItemSlotUIFactory.unload(slot.transform);
                    continue;
                }
                ItemSlotUIFactory.reload(slot,inventory[n]);
            }
        }

        public void unloadItem(int n) {
            ItemSlotUIFactory.unload(slots[n].transform);
        }

        public void reloadItem(int n) {
            ItemSlotUIFactory.reload(slots[n],inventory[n]);
        }
        protected virtual void initSlot(int n) {
            Transform slotTransform = transform.Find("slot" + n);
            if (slotTransform == null) {
                Debug.LogError("slot" + n + " doesn't exist but tried to load it into  inventory " + name);
                slots.Add(null);
                return;
            }
            slots.Add(slotTransform.gameObject);
            initClickHandler(slotTransform,n);
        }

        protected void initClickHandler(Transform slot, int n) {
            
            ItemSlotUIClickHandler clickHandler = slot.GetComponent<ItemSlotUIClickHandler>();
            if (clickHandler == null) {
                Debug.LogError("Slot" + n + " doesn't have click handler");
                return;
            }
            clickHandler.init(this,n);
        }
        public virtual void loadItem(int n) {
            if (n >= slots.Count) {
                return;
            }
            GameObject slot = slots[n];
            ItemSlot itemSlot = inventory[n];
            ItemSlotUIFactory.load(itemSlot,slot.transform);
        }

        public virtual void setItem(int n, ItemSlot data) {
            if (n < 0 || n >= slots.Count) {
                return;
            }
            GameObject slot = slots[n];
            inventory[n]=data;
            loadItem(n);
        }
    

        public abstract void rightClick(int n);
        public abstract void leftClick(int n);
        public abstract void middleClick(int n);
    }

}


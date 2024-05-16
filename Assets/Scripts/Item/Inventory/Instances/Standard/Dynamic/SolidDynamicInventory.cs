using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Inventory {
    public class SolidDynamicInventory : AbstractSolidItemInventory
    {
        private Vector2Int size;
        public int Count {get => size.x * size.y;}
        public Vector2Int Size { get => size; set => size = value; }

        public void initalize(List<ItemSlot> inventory, Vector2Int size) {
            this.inventory = inventory;
            this.size = size;
            initalizeSlots();
        }
        protected override void initSlot(int n)
        {
            Transform slotTransform = transform.Find("slot"+n);
            if (slotTransform != null) {
                slots.Add(slotTransform.GetComponent<ItemSlotUI>());
                initClickHandler(slotTransform,n);
            } else {
                addSlot();
            }
            
        }
        public ItemSlotUI addSlot() {
            if (slots.Count >= size.x * size.y) {
                return null;
            }
            ItemSlotUI slot = ItemSlotUIFactory.newItemSlotUI(null,transform,ItemDisplayUtils.SolidItemPanelColor,suffix:slots.Count.ToString());
            slots.Add(slot);
            slot.gameObject.AddComponent<ItemSlotUIClickHandler>();
            initClickHandler(slot.transform,slots.Count-1);
            return slot;
        }
        public bool popSlot() {
            if (slots.Count <= 0) {
                return false;
            }
            ItemSlotUI slot = slots[slots.Count-1];
            slots.RemoveAt(slots.Count-1);
            GameObject.Destroy(slot.gameObject);
            return true;
        }

        public void updateSize(UnityEngine.Vector2Int newSize) {
            this.size = newSize;
            while (slots.Count < Count) {
                addSlot();
            }
            while (slots.Count > Count) {
                popSlot();
            }
        }

        public void setInventory(List<ItemSlot> itemSlots) {
            this.inventory = itemSlots;
            for (int i = 0; i < itemSlots.Count; i++) {
                if (i >= slots.Count) {
                    Debug.LogWarning("Tried to change inventory to one larger than the current size");
                    break;
                }
                displayItem(i);
            }
        }
    }

}

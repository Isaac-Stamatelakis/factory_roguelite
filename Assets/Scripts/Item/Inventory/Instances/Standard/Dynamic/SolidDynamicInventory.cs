using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Inventory {
    public class SolidDynamicInventory : AbstractSolidItemInventory
    {
        public ItemSlotUI ItemSlotUIPrefab;
        private Vector2Int size;
        public int Count {get => size.x * size.y;}
        public Vector2Int Size { get => size; set => size = value; }

        public void initalize(List<ItemSlot> inventory, Vector2Int size) {
            this.inventory = inventory;
            this.size = size;
            InitalizeSlots();
        }
        protected override void InitSlot(int n)
        {
            if (slots.Count <= n)
            {
                AddSlot();
                return;
            }
            Transform slotTransform = transform.GetChild(n);
            slots.Add(slotTransform.GetComponent<ItemSlotUI>());
            InitClickHandler(slotTransform,n);
            
        }
        public ItemSlotUI AddSlot() {
            if (slots.Count >= size.x * size.y) {
                return null;
            }
            ItemSlotUI slot = Instantiate(ItemSlotUIPrefab, transform);
            slots.Add(slot);
            InitClickHandler(slot.transform,slots.Count-1);
            return slot;
        }
        public bool PopSlot() {
            if (slots.Count <= 0) {
                return false;
            }
            ItemSlotUI slot = slots[slots.Count-1];
            slots.RemoveAt(slots.Count-1);
            GameObject.Destroy(slot.gameObject);
            return true;
        }

        public void UpdateSize(UnityEngine.Vector2Int newSize) {
            this.size = newSize;
            while (slots.Count < Count) {
                AddSlot();
            }
            while (slots.Count > Count) {
                PopSlot();
            }
        }

        public void SetInventory(List<ItemSlot> itemSlots) {
            this.inventory = itemSlots;
            for (int i = 0; i < itemSlots.Count; i++) {
                if (i >= slots.Count) {
                    Debug.LogWarning("Tried to change inventory to one larger than the current size");
                    break;
                }
                DisplayItem(i);
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule.Inventory {
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
            GameObject slot = Global.findChild(transform,"slot"+n);
            if (slot == null) {
                slot = addSlot();
            } else {
                slots.Add(slot);
                initClickHandler(slot.transform,n);
            }
            
        }
        public GameObject addSlot() {
            if (slots.Count >= size.x * size.y) {
                return null;
            }
            GameObject slot = Instantiate(Resources.Load<GameObject>(InventoryHelper.SolidSlotPrefabPath));
            slot.name = "slot" + (slots.Count).ToString();
            slot.transform.SetParent(transform,false);
            slots.Add(slot);
            initClickHandler(slot.transform,slots.Count-1);
            loadItem(slots.Count-1);
            return slot;
        }
        public bool popSlot() {
            if (slots.Count <= 0) {
                return false;
            }
            GameObject slot = slots[slots.Count-1];
            slots.RemoveAt(slots.Count-1);
            Destroy(slot);
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
                GameObject slot = slots[i];
                ItemSlotUIFactory.unload(slot.transform);
                ItemSlotUIFactory.load(itemSlots[i],slot.transform);
            }
        }
    }

}

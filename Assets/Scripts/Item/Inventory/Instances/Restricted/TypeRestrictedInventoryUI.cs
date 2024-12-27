using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Items.Inventory {
    public abstract class TypeRestrictedInventoryUI<T> : AbstractSolidItemInventory
    {
        public void initalize(List<ItemSlot> items)
        {
            this.inventory = items;
            InitalizeSlots();
        }
        public override void leftClick(int n)
        {
            GameObject grabbedItem = GameObject.Find("GrabbedItem");
            if (grabbedItem == null) {
                Debug.LogError("Inventory " + name + " GrabbedItem is null");
            }
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (grabbedSlot != null && grabbedSlot.itemObject != null && grabbedSlot.itemObject is not T) {
                return;
            }
            base.leftClick(n);
        }

        public override void middleClick(int n)
        {
            base.middleClick(n);
        }

        public override void rightClick(int n)
        {
            base.rightClick(n);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItemModule.Tags;

namespace ItemModule.Inventory {
    public class TagRestrictedInventoryUI : AbstractSolidItemInventory
    {
        [SerializeField] protected ItemTag validTag;
        public virtual void initalize(List<ItemSlot> items, ItemTag validTag)
        {
            this.inventory = items;
            this.validTag = validTag;
            initalizeSlots();
        }
        public override void leftClick(int n)
        {
            GameObject grabbedItem = GameObject.Find("GrabbedItem");
            if (grabbedItem == null) {
                Debug.LogError("Inventory " + name + " GrabbedItem is null");
            }
            GrabbedItemProperties grabbedItemProperties = grabbedItem.GetComponent<GrabbedItemProperties>();
            ItemSlot grabbedSlot = grabbedItemProperties.itemSlot;
            if (grabbedSlot != null && grabbedSlot.itemObject != null && (grabbedSlot.tags == null || !grabbedSlot.tags.Dict.ContainsKey(validTag))) {
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


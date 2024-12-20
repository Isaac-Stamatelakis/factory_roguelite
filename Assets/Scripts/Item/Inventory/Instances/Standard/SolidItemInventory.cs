using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Inventory {
    public class SolidItemInventory : AbstractSolidItemInventory, ILoadableInventory
    {
        public void initalize(List<ItemSlot> items)
        {
            this.inventory = items;
            initalizeSlots();
        }
    }
}


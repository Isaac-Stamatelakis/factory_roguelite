using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule.Viewer;

namespace Items.Inventory {
    public class RecipeInventoryUI : InventoryUI, ILoadableInventory
    {
        public void initalize(List<ItemSlot> items)
        {
            this.inventory = items;
            initalizeSlots();
        }

        public override void leftClick(int n)
        {
            ItemSlot itemSlot = inventory[n];
            if (itemSlot == null) {
                return;
            }
            RecipeViewerHelper.displayCraftingOfItem(itemSlot);
        }

        public override void middleClick(int n)
        {
            // Does nothing
        }

        public override void rightClick(int n)
        {
            ItemSlot itemSlot = inventory[n];
            if (itemSlot == null) {
                return;
            }
            RecipeViewerHelper.displayUsesOfItem(itemSlot);
        }
    }
}


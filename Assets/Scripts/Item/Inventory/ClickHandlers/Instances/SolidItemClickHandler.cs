using System.Collections.Generic;
using Items;
using Items.Tags;
using UnityEngine;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class SolidItemClickHandler : ItemSlotUIClickHandler
    {
        public bool RestrictTags;
        public List<ItemTag> WhiteListedTags;
        public bool AllowInputs = true;
        protected override void RightClick() {
            ItemSlot inventorySlot = inventoryUI.GetItemSlot(index);
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            if (inventorySlot == null || inventorySlot.itemObject == null) {
                return;
            }

            var inventory = inventoryUI.Inventory;
            if (!grabbedItemProperties.SetItemSlotFromInventory(inventory,index)) {
                grabbedItemProperties.AddItemSlotFromInventory(inventory,index);
            }
            
            inventoryUI.DisplayItem(index);
        }

        protected override void LeftClick() {
            var inventory = inventoryUI.Inventory;
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            ItemSlot inventorySlot = inventory[index];
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (!AllowInputs && grabbedSlot == null) {
                inventory[index] = null;
                grabbedItemProperties.SetItemSlot(inventorySlot);
                inventoryUI.DisplayItem(index);
                return;
            }
            if (ItemSlotHelper.AreEqual(grabbedSlot,inventorySlot)) {
                // Merge
                uint sum = inventorySlot.amount + grabbedSlot.amount;
                if (sum > Global.MaxSize) {
                    grabbedSlot.amount = sum-Global.MaxSize;
                    inventorySlot.amount = Global.MaxSize;
                } else { // Overflow
                    inventorySlot.amount = sum;
                    grabbedItemProperties.SetItemSlot(null);
                }
            } else {    
                // Swap
                inventory[index] = grabbedItemProperties.ItemSlot;
                grabbedItemProperties.SetItemSlot(inventorySlot);
            }

            inventoryUI.DisplayItem(index);
        }
        
        protected override void MiddleClick()
        {
            
        }
    }
}

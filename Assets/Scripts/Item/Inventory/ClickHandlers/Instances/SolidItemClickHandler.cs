using System.Collections.Generic;
using Item.Slot;
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
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            var inventory = inventoryUI.GetInventory();
            if (!grabbedItemProperties.SetItemSlotFromInventory(inventory,index)) {
                grabbedItemProperties.AddItemSlotFromInventory(inventory,index);
            }
            
            inventoryUI.CallListeners(index);
            inventoryUI.DisplayItem(index);
        }

        protected override void LeftClick() {
            var inventory = inventoryUI.GetInventory();
            inventoryUI.CallListeners(index);
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            ItemSlot inventorySlot = inventory[index];
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (!AllowInputs && grabbedSlot == null) {
                inventory[index] = null;
                grabbedItemProperties.SetItemSlot(inventorySlot);
                inventoryUI.DisplayItem(index);
                inventoryUI.CallListeners(index);
                return;
            }
            if (ItemSlotUtils.AreEqual(grabbedSlot,inventorySlot)) {
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
            inventoryUI.CallListeners(index);
        }
        
        protected override void MiddleClick()
        {
            
        }
    }
}

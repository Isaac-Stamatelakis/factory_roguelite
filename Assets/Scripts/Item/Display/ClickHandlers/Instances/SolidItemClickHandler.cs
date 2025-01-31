using System.Collections.Generic;
using Item.Display.ClickHandlers;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using UnityEngine;

namespace Item.Inventory.ClickHandlers.Instances
{
    public class SolidItemClickHandler : ItemSlotUIClickHandler
    {
        protected override void RightClick() {
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            var inventory = inventoryUI.GetInventory();
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            ItemSlot inventorySlot = inventory[index];
            if (ItemSlotUtils.IsItemSlotNull(inventorySlot))
            {
                
                if (ItemSlotUtils.IsItemSlotNull(grabbedSlot)) return;
                if ((!inventoryUI.ValidateInput(grabbedSlot))) return;
                ItemSlot newSlot = new ItemSlot(grabbedSlot.itemObject, 1, grabbedSlot.tags);
                inventoryUI.SetItem(index,newSlot);
                grabbedItemProperties.SetDeIterateSlot(null);
                grabbedSlot.amount--;
                return;
            }
            if (ItemSlotUtils.IsItemSlotNull(grabbedSlot))
            {
                inventorySlot.amount--;
                ItemSlot newSlot = new ItemSlot(inventorySlot.itemObject, 1, inventorySlot.tags);
                grabbedItemProperties.SetItemSlot(newSlot);
                grabbedItemProperties.SetDeIterateSlot(this);
                inventoryUI.CallListeners(index);
                inventoryUI.DisplayItem(index);
                return;
            }
            
            if (!ItemSlotUtils.AreEqual(inventorySlot, grabbedSlot)) return;
            
            if (grabbedItemProperties.HaveTakenFromSlot(this))
            {
                inventorySlot.amount--; 
                grabbedSlot.amount++;
            }
            else
            {
                inventorySlot.amount++;
                grabbedSlot.amount--;
            }
            
            grabbedItemProperties.UpdateSprite();
            inventoryUI.CallListeners(index);
            inventoryUI.DisplayItem(index);
        }

        protected override void LeftClick()
        {
            
            var inventory = inventoryUI.GetInventory();
            GrabbedItemProperties grabbedItemProperties = GrabbedItemProperties.Instance;
            ItemSlot inventorySlot = inventory[index];
            ItemSlot grabbedSlot = grabbedItemProperties.ItemSlot;
            if (ItemSlotUtils.IsItemSlotNull(grabbedSlot)) {
                // Output 
                inventory[index] = null;
                grabbedItemProperties.SetItemSlot(inventorySlot);
                inventoryUI.DisplayItem(index);
                inventoryUI.CallListeners(index);
                return;
            }
            bool inputEvent = InputFromLeftClick(grabbedSlot,inventorySlot,grabbedItemProperties,inventory);
            if (!inputEvent) return;

            inventoryUI.DisplayItem(index);
            inventoryUI.CallListeners(index);
        }

        private bool InputFromLeftClick(ItemSlot grabbedSlot, ItemSlot inventorySlot, GrabbedItemProperties grabbedItemProperties, List<ItemSlot> inventory)
        {
            if (!inventoryUI.ValidateInput(grabbedSlot) || inventoryUI.InventoryInteractMode == InventoryInteractMode.BlockInput) return false;
            
            if (ItemSlotUtils.AreEqual(grabbedSlot, inventorySlot))
            {
                // Merge
                uint sum = inventorySlot.amount + grabbedSlot.amount;
                if (sum > Global.MaxSize) {
                    grabbedSlot.amount = sum-Global.MaxSize;
                    inventorySlot.amount = Global.MaxSize;
                } else { // Overflow
                    inventorySlot.amount = sum;
                    grabbedItemProperties.SetItemSlot(null);
                }

                return true;
            }
            
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (ReferenceEquals(inventoryUI.Connection, null)) return false;
                var connectionInventory = inventoryUI.Connection.GetInventory();
                if (ItemSlotUtils.CanInsertIntoInventory(connectionInventory,inventory[index],Global.MaxSize))
                {
                    ItemSlotUtils.InsertIntoInventory(connectionInventory, inventory[index], Global.MaxSize);
                    inventoryUI.Connection.RefreshSlots();
                    inventoryUI.Connection.CallListeners(0);
                }
                return true;
            }
            // Swap
            inventory[index] = grabbedItemProperties.ItemSlot;
            grabbedItemProperties.SetItemSlot(inventorySlot);
            return true;
        }
        
        protected override void MiddleClick()
        {
            
        }

        public override void MiddleMouseScroll()
        {
            if (ReferenceEquals(inventoryUI.Connection, null)) return;

            var inventory = inventoryUI.GetInventory();
            ItemSlot inventorySlot = inventory[index];
            if (ItemSlotUtils.IsItemSlotNull(inventorySlot)) return;

            bool isScrollingUp = Input.mouseScrollDelta.y > 0;
            bool takeFromConnection = !isScrollingUp;
            if (takeFromConnection)
            {
                TakeFromConnection();
            }
            else
            {
                MoveToConnection();
            }
        }

        private void TakeFromConnection()
        {
            if (inventoryUI.InventoryInteractMode == InventoryInteractMode.BlockInput) return;
            var inventory = inventoryUI.GetInventory();
            ItemSlot inventorySlot = inventory[index];

            if (inventorySlot.amount >= Global.MaxSize) return;
            
            var connectionInventory = inventoryUI.Connection.GetInventory();
            foreach (ItemSlot connectionSlot in connectionInventory)
            {
                if (ItemSlotUtils.IsItemSlotNull(connectionSlot)) continue;
                if (!ItemSlotUtils.AreEqualNoNullCheck(connectionSlot,inventorySlot)) continue;
                connectionSlot.amount--;
                inventorySlot.amount++;

                CallInventoryUIListeners();
                return;
            }
        }

        private void MoveToConnection()
        {
            var inventory = inventoryUI.GetInventory();
            ItemSlot inventorySlot = inventory[index];
            var connectionInventory = inventoryUI.Connection.GetInventory();
            
            if (!inventoryUI.Connection.ValidateInput(inventorySlot)) return;
            
            int firstNullSlot = -1;
            for (var i = 0; i < connectionInventory.Count; i++)
            {
                var connectionSlot = connectionInventory[i];
                if (ItemSlotUtils.IsItemSlotNull(connectionSlot))
                {
                    if (firstNullSlot < 0)
                    {
                        firstNullSlot = i;
                    }
                    continue;
                }
                if (!ItemSlotUtils.AreEqualNoNullCheck(connectionSlot, inventorySlot)) continue;
                
                connectionSlot.amount++;
                inventorySlot.amount--;
                CallInventoryUIListeners();
                return;
            }

            if (firstNullSlot < 0) return;
            ItemSlot temp = new ItemSlot(inventorySlot.itemObject, 1, inventorySlot.tags);
            inventorySlot.amount--;
            connectionInventory[firstNullSlot] = temp;
            CallInventoryUIListeners();
        }

        private void CallInventoryUIListeners()
        {
            inventoryUI.Connection?.RefreshSlots();
            inventoryUI.Connection?.CallListeners(0);
            inventoryUI.CallListeners(0);
            inventoryUI.RefreshSlots();
        }
    }
}

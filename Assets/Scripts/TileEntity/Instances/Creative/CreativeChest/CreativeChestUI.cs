using System;
using System.Collections.Generic;
using Item.GrabbedItem;
using Item.Slot;
using Items.Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TileEntity.Instances.Creative.CreativeChest
{
    public class CreativeChestUI : MonoBehaviour, ITileEntityUI
    {
        [SerializeField] private InventoryUI mInventoryUI;
        private CreativeChestInstance creativeChestInstance;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not CreativeChestInstance chestInstance) return;
            this.creativeChestInstance = chestInstance;
            mInventoryUI.DisplayInventory(new List<ItemSlot>{creativeChestInstance.ItemSlot});
            mInventoryUI.OverrideClickAction(OnClick);
        }

        private void OnClick(PointerEventData.InputButton inputButton, int index)
        {
            ItemSlot grabbedItemSlot = GrabbedItemProperties.Instance.ItemSlot;
            if (ItemSlotUtils.IsItemSlotNull(grabbedItemSlot))
            {
                ItemSlot creativeSlot = creativeChestInstance.ItemSlot;
                if (ItemSlotUtils.IsItemSlotNull(creativeSlot)) return;
                GrabbedItemProperties.Instance.SetItemSlot(new ItemSlot(creativeSlot.itemObject,Global.MAX_SIZE,creativeSlot.tags));
                creativeSlot.amount = UInt32.MaxValue;
                return;
            }
            creativeChestInstance.ItemSlot = new ItemSlot(grabbedItemSlot.itemObject, uint.MaxValue, grabbedItemSlot.tags);
            mInventoryUI.SetItem(index,creativeChestInstance.ItemSlot);
            mInventoryUI.RefreshSlots();
        }
    }
}

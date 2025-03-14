using System;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using UnityEngine;
using World.Cave.Registry;

namespace TileEntity.Instances.Caves.VoidMiners
{
    public class VoidMinerUI : MonoBehaviour, ITileEntityUI<VoidMinerInstance>
    {
        [SerializeField] private InventoryUI mDriveInventoryUI;
        [SerializeField] private InventoryUI mFilterInventoryUI;
        [SerializeField] private InventoryUI mUpgradeInventoryUI;
        [SerializeField] private InventoryUI mStoneOutputUI;
        [SerializeField] private InventoryUI mOreOutputUI;
        [SerializeField] private InventoryUI mFluidOutputUI;
        private VoidMinerInstance voidMinerInstance;
        public void DisplayTileEntityInstance(VoidMinerInstance tileEntityInstance)
        {
            voidMinerInstance = tileEntityInstance;
            VoidMinerInstance.VoidMinerData minerData = tileEntityInstance.MinerData;
            
            mDriveInventoryUI.DisplayInventory(new List<ItemSlot>{minerData.DriveSlot},clear:false);
            mDriveInventoryUI.AddTagRestriction(ItemTag.CaveData);
            mDriveInventoryUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mDriveInventoryUI.AddCallback(OnDriveSlotChange);

            ItemSlot filterSlot = null;
            if (minerData.ItemFilter != null)
            {
                ItemObject filterItem = ItemRegistry.GetInstance().GetItemObject(ItemTileEntityPort.FILTER_ID);
                filterSlot = new ItemSlot(filterItem, 1, null);
                ItemSlotUtils.AddTag(filterSlot,ItemTag.ItemFilter,minerData.ItemFilter);
            }
            mFilterInventoryUI.DisplayInventory(new List<ItemSlot>{filterSlot},clear:false);
            mFilterInventoryUI.AddTagRestriction(ItemTag.ItemFilter);
            mFilterInventoryUI.SetRestrictionMode(InventoryRestrictionMode.WhiteList);
            mFilterInventoryUI.AddCallback(OnFilterSlotChange);
            
            mUpgradeInventoryUI.gameObject.SetActive(false); // TODO If I still want to add upgrades
            
            mStoneOutputUI.DisplayInventory(minerData.StoneOutputs);
            mStoneOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            mOreOutputUI.DisplayInventory(minerData.OreOutputs);
            mOreOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
            mFluidOutputUI.DisplayInventory(minerData.FluidOutputs);
            mFluidOutputUI.SetInteractMode(InventoryInteractMode.BlockInput);
        }

        public void FixedUpdate()
        {
            mStoneOutputUI.RefreshSlots();
            mOreOutputUI.RefreshSlots();
            mFluidOutputUI.RefreshSlots();
        }

        private void OnDriveSlotChange(int index)
        {
            voidMinerInstance.MinerData.DriveSlot = mDriveInventoryUI.GetItemSlot(index);
            voidMinerInstance.SetCaveTileCollectionFromDriveSlot();
        }

        private void OnFilterSlotChange(int index)
        {
            ItemSlot filterSlot = mFilterInventoryUI.GetItemSlot(index);
            if (filterSlot?.tags?.Dict == null || !filterSlot.tags.Dict.TryGetValue(ItemTag.ItemFilter, out object filterData)) return;
            ItemFilter filter = filterData as ItemFilter;
            if (filterData == null) return;
            voidMinerInstance.MinerData.ItemFilter = filter;
        }
    }
}

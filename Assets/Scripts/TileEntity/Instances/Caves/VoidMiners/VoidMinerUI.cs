using System;
using System.Collections.Generic;
using Conduits.Ports;
using Item.Slot;
using Items;
using Items.Inventory;
using Items.Tags;
using PlayerModule;
using TileEntity.Instances.Machine.UI;
using TMPro;
using UnityEngine;
using World.Cave.Registry;

namespace TileEntity.Instances.Caves.VoidMiners
{
    public class VoidMinerUI : MonoBehaviour, ITileEntityUI, IInventoryUITileEntityUI
    {
        [SerializeField] private TextMeshProUGUI mTitleText;
        [SerializeField] private TextMeshProUGUI mDepthText;
        [SerializeField] private TextMeshProUGUI mDataText;
        
        [SerializeField] private InventoryUI mDriveInventoryUI;
        [SerializeField] private InventoryUI mFilterInventoryUI;
        [SerializeField] private InventoryUI mUpgradeInventoryUI;
        [SerializeField] private VoidMinerOutputUI mStoneOutputUI;
        [SerializeField] private VoidMinerOutputUI mOreOutputUI;
        [SerializeField] private VoidMinerOutputUI mFluidOutputUI;
        private VoidMinerInstance voidMinerInstance;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not VoidMinerInstance voidMiner) return;
            
            voidMinerInstance = voidMiner;
            VoidMinerInstance.VoidMinerData minerData = voidMinerInstance.MinerData;

            mTitleText.text = voidMinerInstance.TileEntityObject.name;
            mDepthText.text = $"Depth: {voidMinerInstance.CompactMachineDepth}";
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
            
            // This hurts me but its slightly more efficent and miners are called very frequently
            mStoneOutputUI.Display(minerData.StoneOutputs,minerData.StoneActive, (state) =>
            {
                minerData.StoneActive = state;
            });
            mOreOutputUI.Display(minerData.OreOutputs,minerData.OreActive, (state) =>
            {
                minerData.OreActive = state;
            });
            mFluidOutputUI.Display(minerData.FluidOutputs,minerData.FluidActive, (state) =>
            {
                minerData.FluidActive = state;
            });
            
        }
        

        private void OnDriveSlotChange(int index)
        {
            voidMinerInstance.MinerData.DriveSlot = mDriveInventoryUI.GetItemSlot(index);
            voidMinerInstance.SetCaveTileCollectionFromDriveSlot(CaveRegistry.Instance);
        }

        private void OnFilterSlotChange(int index)
        {
            ItemSlot filterSlot = mFilterInventoryUI.GetItemSlot(index);
            if (filterSlot?.tags?.Dict == null ||
                !filterSlot.tags.Dict.TryGetValue(ItemTag.ItemFilter, out object filterData))
            {
                voidMinerInstance.MinerData.ItemFilter = null;
                return;
            }

            ItemFilter filter = filterData as ItemFilter;
            if (filterData == null) return;
            voidMinerInstance.MinerData.ItemFilter = filter;
        }

        public InventoryUI GetInput()
        {
            return mDriveInventoryUI;
        }

        public List<InventoryUI> GetAllInventoryUIs()
        {
            return new List<InventoryUI>
            {
                mDriveInventoryUI,
                mFilterInventoryUI,
                mFluidOutputUI.InventoryUI,
                mOreOutputUI.InventoryUI,
                mStoneOutputUI.InventoryUI
            };
        }
    }
}

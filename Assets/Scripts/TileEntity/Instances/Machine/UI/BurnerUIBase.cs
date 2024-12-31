using System;
using System.Collections.Generic;
using Item.Inventory.ClickHandlers.Instances;
using Items;
using Items.Inventory;
using Recipe.Viewer;
using TileEntity.Instances.Machine.Instances.Passive;
using TileEntity.Instances.Machines;
using UnityEngine;

namespace TileEntity.Instances.Machine.UI
{
    public class BurnerUIBase : MonoBehaviour, ITileEntityUI<BurnerMachineInstance>, IRecipeProcessorUI
    {
        [SerializeField] private MachineInventoryUI machineInventoryUI;
        [SerializeField] private InventoryUI burnerInventoryUI;
        [SerializeField] private ArrowProgressController burnerProgress;
        [SerializeField] private ArrowProgressController machineProgress;
        private BurnerMachineInstance displayedInstance;
        public void DisplayTileEntityInstance(BurnerMachineInstance tileEntityInstance)
        {
            displayedInstance = tileEntityInstance;
            machineInventoryUI.Display(tileEntityInstance.GetItemInventory());
            burnerInventoryUI.DisplayInventory(tileEntityInstance.BurnerFuelInventory.BurnerSlots);
            burnerInventoryUI.AddListener(tileEntityInstance);
        }

        public void DisplayRecipe(DisplayableRecipe recipes)
        {
            throw new System.NotImplementedException();
        }

        public void FixedUpdate()
        {
            if (!ReferenceEquals(displayedInstance, null)) machineProgress.SetArrowProgress(displayedInstance.GetProgressPercent());
            if (!ReferenceEquals(displayedInstance,null)) burnerProgress.SetArrowProgress(displayedInstance.BurnerFuelInventory.GetBurnPercent());
        }
    }
}

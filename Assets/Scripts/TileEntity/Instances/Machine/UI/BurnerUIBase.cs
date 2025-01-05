using System;
using System.Collections.Generic;
using Item.Burnables;
using Item.Inventory;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using Items.Inventory;
using Recipe;
using Recipe.Viewer;
using TileEntity.Instances.Machine.Instances.Passive;
using TileEntity.Instances.Machines;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace TileEntity.Instances.Machine.UI
{
    public class BurnerUIBase : MonoBehaviour, ITileEntityUI<BurnerMachineInstance>, IRecipeProcessorUI
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TileEntityInventoryUI tileEntityInventoryUI;
        [SerializeField] private InventoryUI burnerInventoryUI;
        [SerializeField] private ArrowProgressController burnerProgress;
        [SerializeField] private ArrowProgressController machineProgress;
        private BurnerMachineInstance displayedInstance;
        public void DisplayTileEntityInstance(BurnerMachineInstance tileEntityInstance)
        {
            displayedInstance = tileEntityInstance;
            title.text = tileEntityInstance.getName();
            tileEntityInventoryUI.Display(tileEntityInstance.GetItemInventory().Content,tileEntityInstance.GetMachineLayout(),tileEntityInstance);
            burnerInventoryUI.DisplayInventory(tileEntityInstance.BurnerFuelInventory.BurnerSlots);
            burnerInventoryUI.AddListener(tileEntityInstance);
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            title.gameObject.SetActive(false);
            tileEntityInventoryUI.DisplayRecipe(recipe);
            InventoryUIRotator burnerRotator = burnerInventoryUI.GetComponent<InventoryUIRotator>();
            if (ReferenceEquals(burnerRotator, null))
            {
                burnerRotator = burnerInventoryUI.AddComponent<InventoryUIRotator>();
            }

            List<ItemSlot> randomBurnables = RecipeRegistry.BurnableItemRegistry.GetRandomBurnableItems(); 
            List<List<ItemSlot>> burnableInventories = new List<List<ItemSlot>>();
            foreach (ItemSlot itemSlot in randomBurnables)
            {
                burnableInventories.Add(new List<ItemSlot> { itemSlot });
            }
            burnerRotator.Initialize(burnableInventories,1,100);
            burnerInventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
        }

        public void FixedUpdate()
        {
            if (!ReferenceEquals(displayedInstance, null)) machineProgress.SetArrowProgress(displayedInstance.GetProgressPercent());
            if (!ReferenceEquals(displayedInstance,null)) burnerProgress.SetArrowProgress(displayedInstance.BurnerFuelInventory.GetBurnPercent());
        }
    }
}

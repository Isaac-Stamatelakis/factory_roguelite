using System;
using System.Collections.Generic;
using Item.Burnables;
using Item.Inventory;
using Item.Inventory.ClickHandlers.Instances;
using Item.Slot;
using Items;
using Items.Inventory;
using Recipe;
using Recipe.Processor;
using Recipe.Processor.Restrictions;
using Recipe.Restrictions;
using Recipe.Viewer;
using TileEntity.Instances.Machine.Instances.Passive;
using TileEntity.Instances.Machines;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using ItemObject = Items.ItemObject;

namespace TileEntity.Instances.Machine.UI
{
    public class BurnerUIBase : MonoBehaviour, ITileEntityUI, IRecipeProcessorUI, IInventoryUIAggregator
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TileEntityInventoryUI tileEntityInventoryUI;
        [SerializeField] private InventoryUI burnerInventoryUI;
        [SerializeField] private ArrowProgressController burnerProgress;
        [SerializeField] private ArrowProgressController machineProgress;
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        [SerializeField] private TextMeshProUGUI mModeText;
        private BurnerMachineInstance displayedInstance;
        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not BurnerMachineInstance burnerMachineInstance) return;
            displayedInstance = burnerMachineInstance;
            title.text = tileEntityInstance.GetName();
            tileEntityInventoryUI.Display(burnerMachineInstance.GetItemInventory().Content,burnerMachineInstance.GetMachineLayout(),tileEntityInstance);
            burnerInventoryUI.DisplayInventory(burnerMachineInstance.BurnerFuelInventory.BurnerSlots);
            burnerInventoryUI.AddCallback(burnerMachineInstance.InventoryUpdate);

            if (burnerMachineInstance.TileEntityObject.RecipeProcessor.ProcessorRestrictionObject is not RecipeProcessorFuelRestriction fuelRestriction) return;

            bool ValidateFunction(ItemObject itemObject, int index)
            {
                if (!itemObject) return false;
                foreach (ItemObject whiteListedFuel in fuelRestriction.WhiteListedFuels)
                {
                    if (whiteListedFuel?.id == itemObject.id) return true;
                }
                return false;
            }
            burnerInventoryUI.SetInputRestrictionCallBack(ValidateFunction);
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            title.gameObject.SetActive(false);
            tileEntityInventoryUI.DisplayRecipe(recipe);
            amountIteratorUI.gameObject.SetActive(false);
            
            burnerInventoryUI.SetInteractMode(InventoryInteractMode.Recipe);
            
            InventoryUIRotator burnerRotator = burnerInventoryUI.GetComponent<InventoryUIRotator>();
            if (ReferenceEquals(burnerRotator, null))
            {
                burnerRotator = burnerInventoryUI.AddComponent<InventoryUIRotator>();
            }
            List<ItemSlot> burnables = GetDisplayBurnables(recipe);
            List<List<ItemSlot>> burnableInventories = new List<List<ItemSlot>>();
            foreach (ItemSlot itemSlot in burnables)
            {
                burnableInventories.Add(new List<ItemSlot> { itemSlot });
            }
            burnerRotator.Initialize(burnableInventories,1,100);
            
           
        }

        private List<ItemSlot> GetDisplayBurnables(DisplayableRecipe recipe)
        {
            RecipeProcessorRestrictionObject restrictionObject = recipe.RecipeData.ProcessorInstance.RecipeProcessorObject.ProcessorRestrictionObject;
            if (restrictionObject is RecipeProcessorFuelRestriction fuelRestriction)
            {
                List<ItemSlot> restrictedSlots = new List<ItemSlot>();
                foreach (ItemObject itemObject in fuelRestriction.WhiteListedFuels)
                {
                    restrictedSlots.Add(new ItemSlot(itemObject,1,null));
                }
                return restrictedSlots;
            }
            return ItemRegistry.BurnableItemRegistry.GetRandomBurnableItems(); 
        }

        public void FixedUpdate()
        {
            if (!ReferenceEquals(displayedInstance, null)) machineProgress.SetArrowProgress(displayedInstance.GetProgressPercent());
            if (!ReferenceEquals(displayedInstance,null)) burnerProgress.SetArrowProgress(displayedInstance.BurnerFuelInventory.GetBurnPercent());
        }
        
        public IInventoryUITileEntityUI GetUITileEntityUI()
        {
            return tileEntityInventoryUI;
        }
    }
}

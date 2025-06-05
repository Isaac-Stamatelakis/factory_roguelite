using System;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using Items;
using Items.Inventory;
using Recipe.Viewer;
using TileEntity.Instances.Machines;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Machine.UI
{

    public interface IInventoryUITileEntityUI
    {
        public InventoryUI GetInput();
        public List<InventoryUI> GetAllInventoryUIs();
    }

    public interface IInventoryUIAggregator
    {
        public IInventoryUITileEntityUI GetUITileEntityUI();
    }
    
    public interface IMachineInstance : ITileEntityInstance, IInventoryUpdateListener
    {
        public float GetProgressPercent();
        public TileEntityLayoutObject GetMachineLayout();
        public MachineItemInventory GetItemInventory();
        public MachineEnergyInventory GetEnergyInventory();
        public void SetMode(int mode);
        public int GetMode();
        public void IterateMode(int amount);
        public int GetModeCount();
        public void ResetRecipe();
    }
    
    public class MachineBaseUI : MonoBehaviour, ITileEntityUI, IAmountIteratorListener, IRecipeProcessorUI, IInventoryUIAggregator
    {
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private ArrowProgressController progressController;
        [SerializeField] private Scrollbar energyScrollbar;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TileEntityInventoryUI tileEntityInventoryUI;
        [SerializeField] private InventoryUI batteryInventoryUI;

        private MachineEnergyInventory machineEnergyInventory;
        private IMachineInstance displayedInstance;
        public void Start()
        {
            amountIteratorUI.SetListener(this);
        }

        public void DisplayTileEntityInstance(ITileEntityInstance tileEntityInstance)
        {
            if (tileEntityInstance is not IMachineInstance machineInstance) return;
            this.displayedInstance = machineInstance;
            titleText.text = machineInstance.GetTileEntity().name.Replace("_","");
            
            machineEnergyInventory = machineInstance.GetEnergyInventory();
            energyScrollbar.gameObject.SetActive(machineEnergyInventory!=null);
            
            InitializeModeDisplay();
            tileEntityInventoryUI.Display(machineInstance.GetItemInventory().Content,machineInstance.GetMachineLayout(),machineInstance);
            if (machineInstance is not IBatterySlotMachine batterySlotMachine)
            {
                batteryInventoryUI.gameObject.SetActive(false);
            }
            else
            {
                batteryInventoryUI.DisplayInventory(batterySlotMachine.GetBatteryInventory());
                batteryInventoryUI.AddCallback(machineInstance.InventoryUpdate);
            }
        }

        

        private void InitializeModeDisplay()
        {
            int modeCount = displayedInstance.GetModeCount();
            if (modeCount < 2)
            {
                amountIteratorUI.gameObject.SetActive(false);
            }
            else
            {
                modeText.text = displayedInstance.GetMode().ToString();
            }
        }
        
        

        public void Update()
        {
            if (machineEnergyInventory != null)
            {
                energyScrollbar.size = machineEnergyInventory.EnergyInventory.GetFillPercent();
            }

            if (displayedInstance != null)
            {
                progressController.SetArrowProgress(displayedInstance.GetProgressPercent());
            }
            
            
        }

        public void iterate(int amount)
        {
            displayedInstance.IterateMode(amount);
        }

        public void DisplayRecipe(DisplayableRecipe recipe)
        {
            string modeDisplay = $"Mode: {recipe.RecipeData.Mode}";
            if (recipe.RecipeData.ProcessorInstance.HasModeName(recipe.RecipeData.Mode))
            {
                string modeString = recipe.RecipeData.ProcessorInstance.GetModeName(recipe.RecipeData.Mode);
                modeDisplay += $" ({modeString})";
            }

            titleText.text = modeDisplay;
            energyScrollbar.gameObject.SetActive(false);
            tileEntityInventoryUI.DisplayRecipe(recipe);
            batteryInventoryUI.gameObject.SetActive(false);
            amountIteratorUI.gameObject.SetActive(false);
        }
        

        public IInventoryUITileEntityUI GetUITileEntityUI()
        {
            return tileEntityInventoryUI;
        }
    }
}

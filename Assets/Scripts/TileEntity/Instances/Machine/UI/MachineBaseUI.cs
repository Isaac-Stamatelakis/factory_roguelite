using System;
using System.Collections.Generic;
using System.Linq;
using Items.Inventory;
using Recipe.Viewer;
using TileEntity.Instances.Machines;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.Machine.UI
{

    public interface IMachineInstance : ITileEntityInstance, IInventoryListener
    {
        public float GetProgressPercent();
        public MachineLayoutObject GetMachineLayout();
        public MachineItemInventory GetItemInventory();
        public MachineEnergyInventory GetEnergyInventory();
        public void SetMode(int mode);
        public int GetMode();
        public void IterateMode(int amount);
        public int GetModeCount();
        public void ResetRecipe();
    }
    
    public class MachineBaseUI : MonoBehaviour, ITileEntityUI<IMachineInstance>, IAmountIteratorListener
    {
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private ArrowProgressController progressController;
        [SerializeField] private Scrollbar energyScrollbar;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private MachineInventoryUI machineInventoryUI;

        private MachineEnergyInventory machineEnergyInventory;
        private IMachineInstance displayedInstance;
        public void Start()
        {
            amountIteratorUI.setListener(this);
        }

        public void DisplayTileEntityInstance(IMachineInstance machineInstance)
        {
            this.displayedInstance = machineInstance;
            titleText.text = machineInstance.GetTileEntity().name;
            
            machineEnergyInventory = machineInstance.GetEnergyInventory();
            energyScrollbar.gameObject.SetActive(machineEnergyInventory!=null);
            
            InitializeModeDisplay();
            machineInventoryUI.Display(machineInstance.GetItemInventory());
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
        
        

        public void DisplayProcessor(DisplayableRecipe displayableRecipe)
        {
            
        }

        public void Update()
        {
            if (machineEnergyInventory != null)
            {
                energyScrollbar.size = machineEnergyInventory.GetFillPercent();
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
    }
}

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
    }
    
    public class MachineBaseUI : MonoBehaviour, ITileEntityUI<IMachineInstance>, IAmountIteratorListener
    {
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private ArrowProgressController progressController;
        [SerializeField] private Scrollbar energyScrollbar;
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private InventoryUI solidInputs;
        [SerializeField] private InventoryUI solidOutputs;
        [SerializeField] private InventoryUI fluidInputs;
        [SerializeField] private InventoryUI fluidOutputs;

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
            InitializeItemDisplay();
        }

        public void FixedUpdate()
        {
            solidInputs.RefreshSlots();
            solidOutputs.RefreshSlots();
            fluidInputs.RefreshSlots();
            fluidOutputs.RefreshSlots();
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
        private void InitializeItemDisplay()
        {
            MachineLayoutObject layoutObject = displayedInstance.GetMachineLayout();
            bool error = false;
            if (layoutObject == null)
            {
                Debug.LogWarning($"'MachineBaseUI' Tried to display inventory with no layout for {displayedInstance.getName()}");
                error = true;
            }
            MachineItemInventory machineItemInventory = displayedInstance.GetItemInventory();
            if (machineItemInventory == null)
            {
                Debug.LogWarning($"'MachineBaseUI' Tried to display null inventory for {displayedInstance.getName()}");
                error = true;
            }
            if (error) return;
            
            InitializeInventoryUI(solidInputs, machineItemInventory.itemInputs,layoutObject.SolidInputs);
            InitializeInventoryUI(solidOutputs, machineItemInventory.itemOutputs,layoutObject.SolidOutputs);
            InitializeInventoryUI(fluidInputs, machineItemInventory.fluidInputs,layoutObject.FluidInputs);
            InitializeInventoryUI(fluidOutputs, machineItemInventory.fluidOutputs,layoutObject.FluidOutputs);
        }
        private void InitializeInventoryUI(InventoryUI inventoryUI, List<ItemSlot> inventory, MachineInventoryOptions inventoryOptions)
        {
            int size = inventoryOptions.GetIntSize();
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }

            if (inventory == null)
            {
                Debug.LogWarning($"'MachineBaseUI' Tried to display '{inventoryUI.name}' for {displayedInstance.getName()} which was null");
                return;
            }
            if (!inventoryOptions.DefaultOffset)
            {
                inventoryUI.transform.position = (Vector2)inventoryOptions.Offset;
            }
            inventoryUI.DisplayInventory(inventory);
            inventoryUI.AddListener(displayedInstance);
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

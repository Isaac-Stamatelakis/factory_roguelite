using System;
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
    public interface IModeMachine
    {
        public void SetMode();
        public int GetMode();
        public void IterateMode(int amount);
    }

    public interface IMachineInstance : ITileEntityInstance
    {
        public float GetProgressPercent();
        public MachineLayoutObject GetMachineLayout();
    }

    public interface IEnergyTileEntity
    {
        public MachineEnergyInventory GetEnergyInventory();
        
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
            if (machineInstance is IModeMachine modeMachine)
            {
                modeText.text = modeMachine.GetMode().ToString();
            }
            else
            {
                amountIteratorUI.gameObject.SetActive(false);
            }

            if (machineInstance is IEnergyTileEntity energyTileEntity)
            {
                machineEnergyInventory = energyTileEntity.GetEnergyInventory();   
            }
            else
            {
                energyScrollbar.gameObject.SetActive(false);
            }

            MachineLayoutObject layoutObject = machineInstance.GetMachineLayout();
            
        }

        private void InitializeInventoryUI(InventoryUI inventoryUI, MachineInventoryOptions inventoryOptions)
        {
            int size = inventoryOptions.GetIntSize();
            if (size == 0)
            {
                inventoryUI.gameObject.SetActive(false);
                return;
            }
            if (!inventoryOptions.DefaultOffset)
            {
                inventoryUI.transform.position = (Vector2)inventoryOptions.Offset;
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
            ((IModeMachine)displayedInstance).IterateMode(amount);
        }
    }
}

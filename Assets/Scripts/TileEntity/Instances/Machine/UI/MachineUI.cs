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
    }

    public interface IEnergyTileEntity
    {
        public MachineEnergyInventory GetEnergyInventory();
    }
    public class MachineUI : MonoBehaviour, IAmountIteratorListener
    {
        [SerializeField] private AmountIteratorUI amountIteratorUI;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private ArrowProgressController progressController;
        [SerializeField] private Scrollbar energyScrollbar;
        [SerializeField] private SolidDynamicInventory playerInventory;
        [SerializeField] private TextMeshProUGUI modeText;

        private MachineEnergyInventory machineEnergyInventory;
        private IMachineInstance displayedInstance;
        public void Start()
        {
            amountIteratorUI.setListener(this);
        }

        public void DisplayInstance(IMachineInstance machineInstance)
        {
            this.displayedInstance = machineInstance;
            titleText.text = machineInstance.GetTileEntity().name;
            if (machineInstance is IModeMachine modeMachine)
            {
                modeText.text = modeMachine.GetMode().ToString();
            }
            else
            {
                modeText.gameObject.SetActive(false);
            }

            if (machineInstance is IEnergyTileEntity energyTileEntity)
            {
                machineEnergyInventory = energyTileEntity.GetEnergyInventory();   
            }
            else
            {
                energyScrollbar.gameObject.SetActive(false);
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
            progressController.SetArrowProgress(displayedInstance.GetProgressPercent());
        }

        public void iterate(int amount)
        {
            ((IModeMachine)displayedInstance).IterateMode(amount);
        }
    }
}

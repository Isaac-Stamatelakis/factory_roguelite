using Items.Inventory;
using Items.Tags;
using UnityEngine;
using UnityEngine.UI;

namespace TileEntity.Instances.CompactMachine.BluePrinter
{
    public class CompactMachineBluePrinterUI : MonoBehaviour, ITileEntityUI<CompactMachineBluePrinterInstance>
    {
        [SerializeField] private InventoryUI mCompactMachineInput;
        [SerializeField] private InventoryUI mCompactMachineOutput;
        [SerializeField] private InventoryUI mCostInventory;
        [SerializeField] private InventoryUI mItemInput;
        [SerializeField] private Button mSelectButton;
        [SerializeField] private Button mCraftButton;

        private CompactMachineBluePrinterInstance bluePrinterInstance;
        public void DisplayTileEntityInstance(CompactMachineBluePrinterInstance tileEntityInstance)
        {
            bluePrinterInstance = tileEntityInstance;
            var inventory = bluePrinterInstance.BluePrintInventory;
            mCompactMachineInput.AddTagRestriction(ItemTag.CompactMachine);
            mCompactMachineInput.DisplayInventory(inventory.CompactMachineInput);
            
            mCompactMachineInput.DisplayInventory(inventory.CompactMachineOutput);
            mCompactMachineOutput.SetInteractMode(InventoryInteractMode.BlockInput);
            
            mItemInput.DisplayInventory(inventory.ItemInput);
            
            mCostInventory.SetInteractMode(InventoryInteractMode.Recipe);
            
            mCraftButton.onClick.AddListener(TryCraft);
            
        }

        private void TryCraft()
        {
            
        }
    }
}

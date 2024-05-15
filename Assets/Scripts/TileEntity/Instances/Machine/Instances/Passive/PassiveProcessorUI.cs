using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using System;
using RecipeModule;

namespace TileEntityModule.Instances.Machines {
    public class PassiveProcessorUI : MonoBehaviour
    {
        [SerializeField] public TextMeshProUGUI title;
        [SerializeField] public ArrowProgressController arrowProgressController;
        [SerializeField] public Image panel;
        private GameObject slotPrefab;
        private PassiveProcessorInventory machineInventory;
        public void displayMachine(IDisplayableLayout<StandardSolidAndFluidInventory> layout, PassiveProcessorInventory machineInventory, string machineName,IInventoryListener listener) {
            layout.display(transform,machineInventory,InventoryUIMode.Standard,listener);
            this.machineInventory = machineInventory;
            title.text = MachineUIFactory.formatMachineName(machineName);
        }

        public void displayRecipe(IDisplayableLayout<StandardSolidAndFluidInventory> layout, PassiveProcessorInventory machineInventory, string machineName) {
            layout.display(transform,machineInventory,InventoryUIMode.Recipe,null);
            this.machineInventory = machineInventory;
            title.text = MachineUIFactory.formatMachineName(machineName);
            panel.enabled = false;
        }

        private void setArrow() {
            //arrowProgressController.setArrow()
        }
    }   

    public static class PassiveProcessorUIFactory {
        public static PassiveProcessorUI getProcessMachineRecipeUI(GameObject uiPrefab, InventoryLayout layout, IRecipe recipe, string name) {
            PassiveProcessorUI machineUI = ProcessorUIFactory.getMachineUI<PassiveProcessorUI>(uiPrefab,layout,name);
            PassiveProcessorInventory machineInventory = RecipeInventoryFactory.toPassiveInventory(recipe);
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                throw new InvalidOperationException(name + " layout is not standard layout");
            }
            machineUI.displayRecipe(standardLayout, machineInventory, name);
            return machineUI;
        }

        public static PassiveProcessorUI getProcessMachineStandardUI(GameObject uiPrefab, InventoryLayout layout, PassiveProcessorInventory inventory, string name,IInventoryListener listener) {
            PassiveProcessorUI machineUI = ProcessorUIFactory.getMachineUI<PassiveProcessorUI>(uiPrefab,layout,name);
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                throw new InvalidOperationException(name + " layout is not standard layout");
            }
            machineUI.displayMachine(standardLayout, inventory, name, listener);
            return machineUI;
        }

        
    }
}

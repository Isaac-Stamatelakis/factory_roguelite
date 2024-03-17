using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule.Transmutation;
using TileEntityModule.Instances.Machines;
using ItemModule.Inventory;
using TileEntityModule;
using GUIModule;

namespace RecipeModule.Processors {
    /// <summary>
    /// Recipe Processor for processor machines with energy
    /// </summary>
    [CreateAssetMenu(fileName ="RP~Powered Machine Processor",menuName="Crafting/Processor/PoweredMachine")]
    public class AggregatedPoweredMachineProcessor : RecipeProcessorAggregator<StandardMachineInventoryLayout>, IDisplayableProcessor
    {
        [SerializeField] public MachineRecipeProcessor itemRecipeProcessor;
        [SerializeField] public TransmutableRecipeProcessor transmutableRecipeProcessor;
        [SerializeField] public TagRecipeProcessor tagRecipeProcessor;

        public void displayTileEntity(StandardMachineInventory tileEntityInventory, Tier tier, string processorName)
        {
            ProcessMachineUI machineUI = getUI();
            if (machineUI == null) {
                Debug.LogError("Machine Gameobject doesn't have UI component");
                return;
            }
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                Debug.LogError(name + " layout is not standard layout");
                return;
            }
            machineUI.displayMachine(standardLayout, tileEntityInventory, processorName, tier);
            GlobalUIContainer.getInstance().getUiController().setGUI(machineUI.gameObject);
            
        }

        private ProcessMachineUI getUI() {
            GameObject uiPrefab = getUIPrefab();
            if (uiPrefab == null) {
                Debug.LogError("GUI GameObject for Processor:" + name + " is null");
                return null;
            }
            GameObject instantiatedUI = GameObject.Instantiate(uiPrefab);
            ProcessMachineUI machineUI = instantiatedUI.GetComponent<ProcessMachineUI>();
            return machineUI;
        }

        public IMachineRecipe getRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs) {
            IMachineRecipe machineRecipe = null;
            if (itemRecipeProcessor != null) {
                machineRecipe = itemRecipeProcessor.getRecipe(mode,solidInputs,fluidInputs,solidOutputs,fluidOutputs);
                if (machineRecipe != null) {
                    return machineRecipe;
                }
            }
            if (transmutableRecipeProcessor != null) {
                machineRecipe = transmutableRecipeProcessor.getRecipe(mode,solidInputs,fluidInputs,solidOutputs,fluidOutputs);
                if (machineRecipe == null) {
                    return machineRecipe;
                }
            }
            if (tagRecipeProcessor != null) {
                machineRecipe = tagRecipeProcessor.getRecipe(mode,solidInputs,fluidInputs,solidOutputs,fluidOutputs);
            }
            return machineRecipe;
        }

        public override int getRecipeCount()
        {
            int count = 0;
            if (itemRecipeProcessor != null) {
                count += itemRecipeProcessor.getRecipeCount();
            }
            if (transmutableRecipeProcessor != null) {
                count += transmutableRecipeProcessor.getRecipeCount();
            }
            if (tagRecipeProcessor != null) {
                count += tagRecipeProcessor.getRecipeCount();
            }
            return count;
        }

        public override List<IRecipe> getRecipes()
        {
            List<IRecipe> recipes = new List<IRecipe>();
            if (itemRecipeProcessor != null) {
                recipes.AddRange(itemRecipeProcessor.getRecipes());
            }
            if (transmutableRecipeProcessor != null) {
                recipes.AddRange(transmutableRecipeProcessor.getRecipes());
            }
            if (tagRecipeProcessor != null) {
                recipes.AddRange(tagRecipeProcessor.getRecipes());
            }
            return recipes;
        }

        public GameObject getRecipeUI(IRecipe recipe, string processorName)
        {
            ProcessMachineUI machineUI = getUI();
            if (machineUI == null) {
                Debug.LogError("Machine Gameobject doesn't have UI component");
                return null;
            }
            if (layout is not IDisplayableLayout<StandardSolidAndFluidInventory> standardLayout) {
                Debug.LogError(name + " layout is not standard layout");
                return null;
            }
            List<ItemSlot> itemInputs;
            List<ItemSlot> fluidInputs;
            ItemSlotHelper.sortInventoryByState(recipe.getInputs(),out itemInputs,out fluidInputs);
            List<ItemSlot> itemOutputs;
            List<ItemSlot> fluidOutputs;
            ItemSlotHelper.sortInventoryByState(recipe.getOutputs(),out itemOutputs,out fluidOutputs);
            StandardMachineInventory machineInventory = new StandardMachineInventory(
                itemInputs,
                itemOutputs,
                fluidInputs,
                fluidOutputs
            );
            machineUI.displayRecipe(standardLayout,machineInventory, processorName);
            return machineUI.gameObject;
        }
    }
}


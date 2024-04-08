using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RecipeModule.Transmutation;
using TileEntityModule.Instances.Machines;
using ItemModule.Inventory;
using TileEntityModule;
using System;

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

        public void displayTileEntity(StandardMachineInventory tileEntityInventory, Tier tier, string processorName, IInventoryListener listener)
        {
            GameObject ui = MachineUIFactory.getProcessMachineStandardUI(getUIPrefab(),layout,tileEntityInventory,tier,processorName,listener).gameObject;
            GlobalUIContainer.getInstance().getUiController().setGUI(ui);
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
            return MachineUIFactory.getProcessMachineRecipeUI(getUIPrefab(),layout,recipe,processorName).gameObject;
        }
    }
}


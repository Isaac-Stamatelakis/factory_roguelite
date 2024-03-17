using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule;
using TileEntityModule.Instances.Machines;
using System;

namespace RecipeModule {
    public static class RecipeInventoryFactory 
    {
        public delegate TInventory InventoryCreator<TInventory>(List<ItemSlot> itemInputs, List<ItemSlot> itemOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs);

        private static TInventory CreateInventory<TInventory>(IRecipe recipe, InventoryCreator<TInventory> createFunc)
        {
            List<ItemSlot> itemInputs;
            List<ItemSlot> fluidInputs;
            ItemSlotHelper.sortInventoryByState(recipe.getInputs(), out itemInputs, out fluidInputs);
            List<ItemSlot> itemOutputs;
            List<ItemSlot> fluidOutputs;
            ItemSlotHelper.sortInventoryByState(recipe.getOutputs(), out itemOutputs, out fluidOutputs);

            return createFunc(itemInputs, itemOutputs, fluidInputs, fluidOutputs);
        }

        public static StandardMachineInventory toStandard(IRecipe recipe)
        {
            return CreateInventory(recipe, (itemInputs, itemOutputs, fluidInputs, fluidOutputs) =>
                new StandardMachineInventory(itemInputs, itemOutputs, fluidInputs, fluidOutputs));
        }

        public static PassiveProcessorInventory toPassiveInventory(IRecipe recipe)
        {
            return CreateInventory(recipe, (itemInputs, itemOutputs, fluidInputs, fluidOutputs) =>
                new PassiveProcessorInventory(itemInputs, itemOutputs, fluidInputs, fluidOutputs));
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances.Machines;
using TileEntityModule;
using Items.Inventory;
using UI;

namespace RecipeModule {
    [CreateAssetMenu(fileName ="RP~New Energy Recipe Processor",menuName="Crafting/Processor/Energy")]
    public class EnergyRecipeProcessor : DisplayableTypedRecipeProcessor<EnergyRecipeCollection>, IEnergyRecipeProcessor
    {
        public void displayTileEntity(StandardMachineInventory tileEntityInventory, Tier tier, string processorName, IInventoryListener listener)
        {
            GameObject ui = MachineUIFactory.getProcessMachineStandardUI(getUIPrefab(),layout,tileEntityInventory,tier,processorName,listener).gameObject;
            MainCanvasController.Instance.DisplayObject(ui);
        }
        public IEnergyProduceRecipe getEnergyRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            
            foreach (EnergyRecipe recipe in recipeCollectionList[mode].recipes) {
                if (recipe.match(solidInputs,solidOutputs,fluidInputs,fluidOutputs)) {
                    return recipe;
                }
            }
            
            return null;
        }

        public override GameObject getRecipeUI(IRecipe recipe, string processorName)
        {
            return MachineUIFactory.getProcessMachineRecipeUI(getUIPrefab(),layout,recipe,processorName).gameObject;
        }
    }
}
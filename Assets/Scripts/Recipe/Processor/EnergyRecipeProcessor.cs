using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    [CreateAssetMenu(fileName ="RP~New Energy Recipe Processor",menuName="Crafting/Processor/Energy")]
    public class EnergyRecipeProcessor : TypedRecipeProcessor<EnergyRecipe>, IEnergyRecipeProcessor
    {
        public IEnergyProduceRecipe getEnergyRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            
            foreach (EnergyRecipe recipe in recipesOfMode[mode]) {
                if (recipe.match(solidInputs,solidOutputs,fluidInputs,fluidOutputs)) {
                    return recipe;
                }
            }
            
            return null;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    [CreateAssetMenu(fileName ="RP~New Item Recipe Processor",menuName="Crafting/Processor/Machine")]
    public class MachineRecipeProcessor : TypedRecipeProcessor<MachineRecipeCollection>, IMachineRecipeProcessor
    {

        public IMachineRecipe getRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs)
        {
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            if (!itemsNotAllNull(solidInputs) && !itemsNotAllNull(fluidInputs)) {
                return null;
            }
            foreach (Recipe recipe in recipesOfMode[mode].recipes) {
                //if (recipe.match(solidInputs,solidOutputs,fluidInputs,fluidOutputs)) {
                //    return (IItemRecipe) recipe;
                //}
            }
            return null;
        }
    }

}

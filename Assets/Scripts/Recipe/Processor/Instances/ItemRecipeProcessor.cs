using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    [CreateAssetMenu(fileName ="RP~New Item Recipe Processor",menuName="Crafting/Processor/Item")]
    public class ItemRecipeProcessor : TypedRecipeProcessor<EnergyRecipeCollection>, IItemRecipeProcessor
    {
        public IItemRecipe getItemRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
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

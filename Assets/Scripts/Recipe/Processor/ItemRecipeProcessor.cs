using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    [CreateAssetMenu(fileName ="RP~New Item Recipe Processor",menuName="Crafting/Processor/Item")]
    public class ItemRecipeProcessor : TypedRecipeProcessor<IItemRecipe>, IItemRecipeProcessor
    {
        public IItemRecipe getItemRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
        {
            
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            if (!itemsNotAllNull(solidInputs) && !itemsNotAllNull(fluidInputs)) {
                return null;
            }
            foreach (IItemRecipe recipe in recipesOfMode[mode]) {
                if (recipe.match(solidInputs,solidOutputs,fluidInputs,fluidOutputs)) {
                    return (IItemRecipe) recipe;
                }
            }
            return null;
        }
    }

}

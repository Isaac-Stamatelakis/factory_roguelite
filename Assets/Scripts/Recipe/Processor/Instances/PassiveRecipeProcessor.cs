using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IPassiveRecipeProcessor {
        public IPassiveRecipe GetPassiveRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs);
    }
    [CreateAssetMenu(fileName ="RP~New Passive Recipe Processor",menuName="Crafting/Processor/Passive")]
    public class PassiveRecipeProcessor : TypedRecipeProcessor<PassiveRecipeCollection>, IPassiveRecipeProcessor
    {
        public IPassiveRecipe GetPassiveRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOutputs)
        {
            
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            if (!itemsNotAllNull(solidInputs) && !itemsNotAllNull(fluidInputs)) {
                return null;
            }
            foreach (PassiveRecipe recipe in recipesOfMode[mode].recipes) {
                if (recipe.match(solidInputs,solidOutputs,fluidInputs,fluidOutputs)) {
                    return recipe;
                }
            }
            return null;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public class EnergyRecipeProcessor : TypedRecipeProcessor<EnergyRecipe>, IEnergyRecipeProcessor
    {
        public IEnergyProduceRecipe getEnergyRecipe(int mode, List<ItemSlot> items, List<ItemSlot> fluids)
        {
            if (!recipesOfMode.ContainsKey(mode)) {
                return null;
            }
            List<ItemSlot> merged = new List<ItemSlot>();
            merged.AddRange(items);
            merged.AddRange(fluids);
            foreach (EnergyRecipe recipe in recipesOfMode[mode]) {
                if (recipe.match(merged)) {
                    return recipe;
                }
            }
            return null;
        }
    }
}
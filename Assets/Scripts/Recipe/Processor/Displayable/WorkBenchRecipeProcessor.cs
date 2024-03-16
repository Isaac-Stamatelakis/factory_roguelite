using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IWorkBenchRecipeProcess {
        public List<WorkbenchRecipe> getCraftableRecipes(int mode,List<ItemSlot> solidInputs, List<ItemSlot> playerInventory);
    }
    [CreateAssetMenu(fileName ="RP~New WorkBench Recipe Processor",menuName="Crafting/Processor/WorkBench")]
    public class WorkBenchRecipeProcessor : DisplayableTypedRecipeProcessor<WorkBenchRecipeCollection>, IWorkBenchRecipeProcess
    {
        /// <summary>
        /// returns all recipes which are currently craftable by the player
        /// </summary>
        public List<WorkbenchRecipe> getCraftableRecipes(int mode,List<ItemSlot> solidInputs, List<ItemSlot> playerInventory) {
            if (!recipesOfMode.ContainsKey(mode)) {
                return new List<WorkbenchRecipe>();
            }
            List<WorkbenchRecipe> validRecipes = new List<WorkbenchRecipe>();
            foreach (WorkbenchRecipe recipe in recipesOfMode[mode].recipes) {
                if (!recipe.match(solidInputs,playerInventory)) {
                    continue;
                }
                validRecipes.Add(recipe);
            }
            return validRecipes;
        }
    }
}


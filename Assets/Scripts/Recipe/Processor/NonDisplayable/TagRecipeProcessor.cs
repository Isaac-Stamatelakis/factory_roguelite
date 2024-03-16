using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule.Processors {
    public class TagRecipeProcessor : RecipeProcessor, ITagRecipeProcessor
    {
        public IMachineRecipe getRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs)
        {
            throw new System.NotImplementedException();
        }

        public override int getRecipeCount()
        {
            throw new System.NotImplementedException();
        }

        public override List<Recipe> getRecipes()
        {
            throw new System.NotImplementedException();
        }
    }
}


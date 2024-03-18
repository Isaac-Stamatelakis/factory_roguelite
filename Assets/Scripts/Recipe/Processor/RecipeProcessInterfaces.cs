using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IEnergyRecipeProcessor {
        public IEnergyProduceRecipe getEnergyRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs);
    }

    public interface ITransmutableRecipeProcessor : IMachineRecipeProcessor{

    }

    public interface IMachineRecipeProcessor {
        public IMachineRecipe getRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs);
    }
    public interface ITagRecipeProcessor : IMachineRecipeProcessor {
        
    }
}
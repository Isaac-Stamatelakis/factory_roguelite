using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IEnergyRecipeProcessor {
        public IEnergyProduceRecipe getEnergyRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs);
    }

    public interface IItemRecipeProcessor {
        public IItemRecipe getItemRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs);
    }

    public interface ITransmutableRecipeProcessor {
        public TransmutableRecipe getValidRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs);
    }
}
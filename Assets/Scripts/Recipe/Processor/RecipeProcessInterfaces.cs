using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IEnergyRecipeProcessor {
        public IItemRecipe getEnergyRecipe(int mode, List<ItemSlot> items, List<ItemSlot> fluids);
    }

    public interface IItemRecipeProcessor {
        public IItemRecipe getItemRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs);
    }

    public interface ITransmutableRecipeProcessor {
        public TransmutableRecipe getValidRecipe(int mode, List<ItemSlot> solidInputs, List<ItemSlot> fluidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidOuputs);
    }
}
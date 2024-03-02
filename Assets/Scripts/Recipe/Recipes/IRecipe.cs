using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IMatchableRecipe { 
        
    }

    public interface IEnergyRecipe {
        public int getRequiredEnergy();
        public int getEnergyPerTick();
    }

    public interface IItemRecipe {
        public List<ItemSlot> getOutputs();
        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOuputs);
    }
}
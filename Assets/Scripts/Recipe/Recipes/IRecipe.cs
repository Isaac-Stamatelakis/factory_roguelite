using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IMatchableRecipe { 
        
    }

    public interface IEnergyProduceRecipe {
        public int getEnergyPerTick();
        public int getLifespan();
    }

    public interface IEnergyConsumeRecipe {
        public int getEnergyCostPerTick();
        public int getTotalEnergyCost();
    }
    public interface IItemRecipe {
        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOuputs);
    }
}
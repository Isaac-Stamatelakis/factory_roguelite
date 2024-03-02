using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public class TransmutableRecipe : IMachineRecipe
    {
        public TransmutableRecipe(ItemSlot output, int requiredEnergy, int energyPerTick) {
            this.output = output;
            this.requiredEnergy = requiredEnergy;
            this.energyPerTick = energyPerTick;
        }
        private ItemSlot output;
        private int requiredEnergy;
        private int energyPerTick;
        public List<ItemSlot> getOutputs()
        {
            return new List<ItemSlot> {
                output
            };
        }

        public int getRequiredEnergy()
        {
            return requiredEnergy;
        }

        public int getEnergyPerTick()
        {
            return energyPerTick;
        }

        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOuputs)
        {
            throw new System.NotImplementedException();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public class TransmutableRecipe : IMachineRecipe
    {
        public TransmutableRecipe(ItemSlot input, ItemSlot output, int requiredEnergy, int energyPerTick) {
            this.input = input;
            this.output = output;
            this.requiredEnergy = requiredEnergy;
            this.energyPerTick = energyPerTick;
        }
        private ItemSlot input;
        private ItemSlot output;
        private int requiredEnergy;
        private int energyPerTick;
        public List<ItemSlot> getOutputs()
        {
            return new List<ItemSlot> {
                output
            };
        }


        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOuputs)
        {
            throw new System.NotImplementedException();
        }

        public int getEnergyCostPerTick()
        {
            return requiredEnergy;
        }

        public int getTotalEnergyCost()
        {
            return energyPerTick;
        }

        public List<ItemSlot> getInputs()
        {
            return new List<ItemSlot>{input};
        }
    }
}

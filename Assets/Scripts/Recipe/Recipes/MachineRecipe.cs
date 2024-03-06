using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RecipeModule {
    [CreateAssetMenu(fileName ="R~New Machine Recipe",menuName="Crafting/Recipe/Machine")]
    public class MachineRecipe : Recipe, IMachineRecipe
    {
        [Header("Energy required to complete recipe")]
        public int totalEnergy;
        [Header("Energy cost per tick")]
        public int energyPerTick;

        public int getEnergyCostPerTick()
        {
            return energyPerTick;
        }

        public int getTotalEnergyCost()
        {
            return totalEnergy;
        }

        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            return RecipeHelper.matchSolidsAndFluids(solidInputs,solidOutputs,fluidInputs,fluidOutputs,inputs,outputs);
        }
    }
}


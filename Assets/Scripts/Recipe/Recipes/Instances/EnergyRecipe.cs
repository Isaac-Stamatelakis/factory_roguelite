using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RecipeModule {
    [CreateAssetMenu(fileName = "R~New Energy Recipe", menuName = "Crafting/Recipe/Energy")]
    public class EnergyRecipe : MultiOutputRecipe, IGeneratorRecipe
    {
        public int energyPerTick;
        public int lifespan;
        public int getEnergyPerTick()
        {
            return energyPerTick;
        }

        public int getLifespan()
        {
            return lifespan;
        }
        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            return RecipeHelper.matchSolidsAndFluids(solidInputs,solidOutputs,fluidInputs,fluidOutputs,inputs,outputs);
        }
    }
}

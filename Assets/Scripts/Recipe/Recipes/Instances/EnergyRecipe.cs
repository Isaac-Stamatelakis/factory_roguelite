using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    [CreateAssetMenu(fileName = "R~New Energy Recipe", menuName = "Crafting/Recipe/Energy")]
    public class EnergyRecipe : Recipe, IEnergyProduceRecipe
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

        public bool match(List<ItemSlot> givenInputs)
        {
            return RecipeHelper.matchInputs(givenInputs,inputs);
        }
    }
}

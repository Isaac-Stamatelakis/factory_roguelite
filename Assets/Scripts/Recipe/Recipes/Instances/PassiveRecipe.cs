using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IPassiveRecipe : IItemRecipe {
        public int getRequiredTicks();
    }
    [CreateAssetMenu(fileName ="R~New Machine Recipe",menuName="Crafting/Recipe/Passive")]
    public class PassiveRecipe : MultiOutputRecipe, IPassiveRecipe
    {
        [SerializeField] public int requiredTicks;
        public int getRequiredTicks()
        {
            return requiredTicks;
        }

        public bool match(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs)
        {
            return RecipeHelper.matchSolidsAndFluids(solidInputs,solidOutputs,fluidInputs,fluidOutputs,inputs,outputs);
        }
    }
}


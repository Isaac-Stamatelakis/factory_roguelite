using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public abstract class ItemOutputRecipe : Recipe
    {
        public List<ItemSlot> outputs;
        [HideInInspector] public List<string> outputGUIDs;
        public List<string> OutputPaths {get{return outputGUIDs;} set{outputGUIDs = value;}}
        public List<ItemSlot> getOutputs()
        {
            return outputs;
        }
        public virtual bool match(List<ItemSlot> givenInputs, List<ItemSlot> givenOutputs) {
            int requiredOutputSpace = outputs.Count;
            if (requiredOutputSpace >= givenOutputs.Count) {
                // No space for recipe
                return false;
            }
            // Quick check O(n)
            if (givenInputs.Count < inputs.Count) {
                return false;
            }
            return RecipeHelper.matchInputs(givenInputs,inputs);
        }
    }
}


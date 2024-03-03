using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeModule {
    public interface IMachineRecipe : IEnergyConsumeRecipe, IItemRecipe {

    }

    public interface IRecipe {

    }
    public abstract class Recipe : ScriptableObject, IRecipe
    {
        public List<ItemSlot> inputs;
        
        // Enable loading of items which have been deleted and recreated (such as transmutables)
        [HideInInspector] public List<string> inputGUIDs;
        
        public List<string> InputPaths {get{return inputGUIDs;} set{inputGUIDs = value;}}
    }   

    public static class RecipeHelper {
        public static bool matchInputs(List<ItemSlot> inputs, List<ItemSlot> recipeItems) {
            // Check recipes O(n+m)
            var inputItemAmounts = new Dictionary<string, int>();
            foreach (ItemSlot itemSlot in inputs) {
                if (itemSlot.itemObject != null) {
                    if (inputItemAmounts.ContainsKey(itemSlot.itemObject.id)) {
                        inputItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
                    } else {
                        inputItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
                    }
                }
            }
            var recipeItemAmounts = new Dictionary<string, int>();
            foreach (ItemSlot itemSlot in recipeItems) {
                if (itemSlot.itemObject != null) {
                    if (recipeItemAmounts.ContainsKey(itemSlot.itemObject.id)) {
                        recipeItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
                    } else {
                        recipeItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
                    }
                }
            }
            foreach (string id in inputItemAmounts.Keys) {
                if (recipeItemAmounts.ContainsKey(id)) {
                    recipeItemAmounts[id] -= inputItemAmounts[id];
                }
            }
            bool success = true;
            foreach (int amount in recipeItemAmounts.Values) {
                if (amount > 0) {
                    success = false;
                    break;
                }
            }
            return success;
        }
    }
}
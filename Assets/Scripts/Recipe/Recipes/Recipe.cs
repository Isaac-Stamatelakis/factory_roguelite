using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RecipeModule {
    public interface IMachineRecipe : IEnergyConsumeRecipe, IItemRecipe {

    }

    public interface IGeneratorRecipe : IEnergyProduceRecipe, IItemRecipe {
        
    }

    public interface IRecipe {

    }
    public abstract class Recipe : ScriptableObject, IRecipe
    {
        
        public List<ItemSlot> inputs;
        [HideInInspector] public List<string> inputGUIDs;
        
        
        public List<string> InputPaths {get{return inputGUIDs;} set{inputGUIDs = value;}}
        
    }   
    public abstract class MultiOutputRecipe : Recipe {
        
        public List<ItemSlot> outputs;
        [HideInInspector] public List<string> outputGUIDs;
        public List<ItemSlot> getOutputs()
        {
            return outputs;
        }
        public List<string> OutputPaths {get{return outputGUIDs;} set{outputGUIDs = value;}}
        
        // Enable loading of items which have been deleted and recreated (such as transmutables)
        
    }
    public abstract class SingleOutputRecipe : Recipe {
        public ItemSlot output;
        public string outputGUID;
    }
    public static class RecipeHelper {
        public static bool matchInputs(List<ItemSlot> inputs, List<ItemSlot> recipeItems) {
            // Check recipes O(n+m)
            var inputItemAmounts = new Dictionary<string, int>();
            foreach (ItemSlot itemSlot in inputs) {
                if (itemSlot == null || itemSlot.itemObject == null) {
                    continue;
                }
                if (inputItemAmounts.ContainsKey(itemSlot.itemObject.id)) {
                    inputItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
                } else {
                    inputItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
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
        public static bool spaceInOutput(List<ItemSlot> inventorySlotOfState, List<ItemSlot> recipeItemsOfState) {
            Dictionary<string, ItemSlot> outputDict = new Dictionary<string, ItemSlot>();
            foreach (ItemSlot itemSlot in recipeItemsOfState) {
                outputDict[itemSlot.itemObject.id] = itemSlot;
            }
            int clearSpaces = 0;
            foreach (ItemSlot itemSlot in inventorySlotOfState) {
                if (itemSlot == null || itemSlot.itemObject == null) {
                    clearSpaces++;
                    continue;
                }
                string id = itemSlot.itemObject.id;
                if (!outputDict.ContainsKey(id)) {
                    continue;
                }
                if (outputDict[id].amount + itemSlot.amount <= Global.MaxSize) { // TODO NBT MATCH
                    outputDict.Remove(id);
                }
            }
            int remaining = outputDict.Count;
            return remaining <= clearSpaces;
        }

        public static void consumeRecipe(List<ItemSlot> inputs, List<ItemSlot> recipeInputs) {
            Dictionary<string, ItemSlot> inputDict = new Dictionary<string, ItemSlot>();
            foreach (ItemSlot itemSlot in inputs) {
                if (itemSlot == null || itemSlot.itemObject == null) {
                    continue;
                }
                if (inputDict.ContainsKey(itemSlot.itemObject.id)) {
                    inputDict[itemSlot.itemObject.id].amount += itemSlot.amount;
                    itemSlot.amount = 0;
                    itemSlot.itemObject = null;
                } else {
                    inputDict[itemSlot.itemObject.id] = itemSlot;
                }
            }
            foreach (ItemSlot itemSlot in recipeInputs) {
                inputDict[itemSlot.itemObject.id].amount -= itemSlot.amount;
            }
        }

        public static bool matchSolidsAndFluids(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs, List<ItemSlot> inputs, List<ItemSlot> outputs) {
            List<ItemSlot> invInputs = (solidInputs ?? Enumerable.Empty<ItemSlot>())
                .Concat(fluidInputs ?? Enumerable.Empty<ItemSlot>())
                .ToList();
            if (!RecipeHelper.matchInputs(invInputs, inputs)) {
                return false;
            }
            List<ItemSlot> invOutputs = (solidOutputs ?? Enumerable.Empty<ItemSlot>())
                .Concat(fluidOutputs ?? Enumerable.Empty<ItemSlot>())
                .ToList();
            if(!RecipeHelper.spaceInOutput(invOutputs, outputs)) {
                return false;
            }
            RecipeHelper.consumeRecipe(invInputs,inputs);
            return true;
        }
    }
}
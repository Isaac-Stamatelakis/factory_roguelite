using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace RecipeModule {
    public interface IMachineRecipe : IEnergyConsumeRecipe, IItemRecipe, IRecipe {

    }

    public interface IGeneratorRecipe : IEnergyProduceRecipe, IItemRecipe, IRecipe {
        
    }

    public interface IRecipe {
        public List<ItemSlot> getOutputs();
        public List<ItemSlot> getInputs();
    }
    public abstract class Recipe : ScriptableObject, IRecipe
    {
        
        public List<ItemSlot> inputs;
        [HideInInspector] public List<string> inputGUIDs;
        public List<ItemSlot> getInputs() {
            return inputs;
        }
        public abstract List<ItemSlot> getOutputs();
        
        public List<string> InputPaths {get{return inputGUIDs;} set{inputGUIDs = value;}}
        
    }   
    public abstract class MultiOutputRecipe : Recipe {
        
        public List<ItemSlot> outputs;
        [HideInInspector] public List<string> outputGUIDs;
        public override List<ItemSlot> getOutputs()
        {
            List<ItemSlot> copy = new List<ItemSlot>();
            foreach (ItemSlot itemSlot in outputs) {
                copy.Add(ItemSlotFactory.copy(itemSlot));
            }
            return copy;
        }
        public List<string> OutputPaths {get{return outputGUIDs;} set{outputGUIDs = value;}}
    }
    public abstract class SingleOutputRecipe : Recipe {
        public ItemSlot output;
        public string outputGUID;
        public override List<ItemSlot> getOutputs()
        {
            return new List<ItemSlot>{output};
        }
    }
    public static class RecipeHelper {
        public static bool matchInputs(List<ItemSlot> inputs, List<ItemSlot> recipeItems) {
            if (inputs == null) {
                if (recipeItems.Count == 0) {
                    return true;
                }
                return false;
            }
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
            if (inventorySlotOfState == null) {
                if (recipeItemsOfState.Count == 0) {
                    return true;
                }
                return false;
            }
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
            if (inputs == null) {
                return;
            }
            Dictionary<string, int> requiredAmount = new Dictionary<string, int>();
            for (int i = 0; i < recipeInputs.Count; i++) {
                ItemSlot recipeInput = recipeInputs[i];
                if (!requiredAmount.ContainsKey(recipeInput.itemObject.id)) {
                    requiredAmount[recipeInput.itemObject.id] = 0;
                }
                requiredAmount[recipeInput.itemObject.id] += recipeInput.amount;
            }
            for (int i = 0; i < inputs.Count; i++) {
                ItemSlot inputSlot = inputs[i];
                if (inputSlot == null || inputSlot.itemObject == null) {
                    continue;
                }
                string id = inputSlot.itemObject.id;
                if (!requiredAmount.ContainsKey(id)) {
                    continue;
                }
                int removal = Mathf.Min(requiredAmount[id],Global.MaxSize);
                inputSlot.amount -= removal;
                requiredAmount[id] -= removal;
                if (requiredAmount[id] == 0) {
                    requiredAmount.Remove(id);
                }
                if (inputSlot.amount == 0) {
                    Debug.Log(i);
                    inputs[i] = null;
                }   
            }
        }

        public static bool matchSolidsAndFluids(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs, List<ItemSlot> inputs, List<ItemSlot> outputs) {
            List<ItemSlot> solidRecipeInputs;
            List<ItemSlot> fluidRecipeInputs;
            ItemSlotHelper.sortInventoryByState(inputs,out solidRecipeInputs,out fluidRecipeInputs);
            if (!RecipeHelper.matchInputs(solidInputs, solidRecipeInputs)) {
                return false;
            }
            if (!RecipeHelper.matchInputs(fluidInputs,fluidRecipeInputs)) {
                return false;
            }
            
            List<ItemSlot> solidRecipeOutputs;
            List<ItemSlot> fluidRecipeOutputs;
            
            ItemSlotHelper.sortInventoryByState(outputs,out solidRecipeOutputs,out fluidRecipeOutputs);
            if(!RecipeHelper.spaceInOutput(solidOutputs, solidRecipeOutputs)) {
                return false;
            }
            if(!RecipeHelper.spaceInOutput(fluidOutputs, fluidRecipeOutputs)) {
                return false;
            }
            RecipeHelper.consumeRecipe(solidInputs,solidRecipeInputs);
            RecipeHelper.consumeRecipe(fluidInputs,solidRecipeInputs);
            return true;
        }

        
    }
}
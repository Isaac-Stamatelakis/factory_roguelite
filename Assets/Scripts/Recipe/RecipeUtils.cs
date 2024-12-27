using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Items;
using Items.Transmutable;
using Recipe;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using UnityEngine;

namespace RecipeModule
{
    
public static class RecipeUtils {
        public static bool MatchInputs(List<ItemSlot> inputs, List<ItemSlot> recipeItems) {
            if (inputs == null) {
                if (recipeItems.Count == 0) {
                    return true;
                }
                return false;
            }
            // Check recipes O(n+m)
            var inputItemAmounts = new Dictionary<string, uint>();
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
            var recipeItemAmounts = new Dictionary<string, uint>();
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
        public static bool SpaceInOutput(List<ItemSlot> inventorySlotOfState, List<ItemSlot> recipeItemsOfState) {
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

        public static void ConsumeRecipe(List<ItemSlot> inputs, List<ItemSlot> recipeInputs) {
            if (inputs == null) {
                return;
            }
            Dictionary<string, uint> requiredAmount = new Dictionary<string, uint>();
            for (int i = 0; i < recipeInputs.Count; i++) {
                ItemSlot recipeInput = recipeInputs[i];
                requiredAmount.TryAdd(recipeInput.itemObject.id, 0);
                requiredAmount[recipeInput.itemObject.id] += recipeInput.amount;
            }
            for (int i = 0; i < inputs.Count; i++) {
                ItemSlot inputSlot = inputs[i];
                if (inputSlot == null || inputSlot.itemObject == null) {
                    continue;
                }
                string id = inputSlot.itemObject.id;
                if (!requiredAmount.TryGetValue(id, value: out var value)) {
                    continue;
                }

                uint removal = GlobalHelper.MinUInt(value, Global.MaxSize);
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

        public static bool MatchSolidsAndFluids(List<ItemSlot> solidInputs, List<ItemSlot> solidOutputs, List<ItemSlot> fluidInputs, List<ItemSlot> fluidOutputs, List<ItemSlot> inputs, List<ItemSlot> outputs) {
            ItemSlotHelper.sortInventoryByState(inputs,out var solidRecipeInputs,out var fluidRecipeInputs);
            if (MatchInputs(solidInputs, solidRecipeInputs)) {
                return false;
            }
            if (MatchInputs(fluidInputs,fluidRecipeInputs)) {
                return false;
            }
            ItemSlotHelper.sortInventoryByState(outputs,out var solidRecipeOutputs,out var fluidRecipeOutputs);
            if(!RecipeUtils.SpaceInOutput(solidOutputs, solidRecipeOutputs)) {
                return false;
            }
            if(!RecipeUtils.SpaceInOutput(fluidOutputs, fluidRecipeOutputs)) {
                return false;
            }
            ConsumeRecipe(solidInputs,solidRecipeInputs);
            ConsumeRecipe(fluidInputs,solidRecipeInputs);
            return true;
        }
        
        public static void InsertValidRecipes(RecipeProcessor recipeProcessor, RecipeObject recipeObject, List<RecipeObject> recipes)
        {
            RecipeType recipeType = recipeProcessor.RecipeType;
            switch (recipeType)
            {
                case RecipeType.Item:
                    if (recipeObject is not ItemRecipeObject && recipeObject is not TransmutableRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.PassiveItem:
                    if (recipeObject is not PassiveItemRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.Generator:
                    if (recipeObject is not PassiveItemRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.EnergyItem:
                    if (recipeObject is not ItemEnergyRecipeObject && recipeObject is not TransmutableRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);

            }
            recipes.Add(recipeObject);
        }

        private static void LogInvalidRecipeWarning(RecipeType recipeType, RecipeProcessor recipeProcessor,
            RecipeObject recipeObject)
        {
            Debug.LogWarning($"Invalid recipe '{recipeObject.name}' not of type '{recipeType} Recipe' in recipe processor {recipeProcessor.name}");
        }

        public static ulong HashItemInput(ItemSlot itemSlot)
        {
            return HashString(itemSlot.itemObject.id);
        }

        private static ulong HashString(string id)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(id));
            return BitConverter.ToUInt64(hashBytes, 0);
        }
        
        public static ulong HashItemInputs(List<ItemSlot> inputs)
        {
            string id = "";
            for (int i = 0; i < inputs.Count; i++)
            {
                ItemSlot itemSlot = inputs[i];
                if (itemSlot == null || itemSlot.itemObject == null)
                {
                    continue;
                }
                id += itemSlot.itemObject.id;
                if (i < inputs.Count - 1)
                {
                    id += "Z"; // ids will never contain this char
                }
            }
            return HashString(id);
        }
        
        public static ulong HashItemInputs(List<ItemSlot> first, List<ItemSlot> second)
        {
            string id = "";
            for (int i = 0; i < first.Count; i++)
            {
                ItemSlot itemSlot = first[i];
                if (itemSlot == null || itemSlot.itemObject == null)
                {
                    continue;
                }
                id += itemSlot.itemObject.id;
                if (second.Count > 0 || i < first.Count - 1)
                {
                    id += "Z";
                }
            }

            for (int i = 0; i < second.Count; i++)
            {
                ItemSlot itemSlot = second[i];
                if (itemSlot == null || itemSlot.itemObject == null)
                {
                    continue;
                }
                id += itemSlot.itemObject.id;
                if (i < second.Count - 1)
                {
                    id += "Z";
                }
            }
            return HashString(id);
        }
    }
}

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

        public static bool OutputsUsed(ItemRecipe itemRecipe)
        {
            return ItemSlotHelper.IsEmpty(itemRecipe.SolidOutputs) && ItemSlotHelper.IsEmpty(itemRecipe.FluidOutputs);
        }
        
    
        
        public static void InsertValidRecipes(RecipeProcessor recipeProcessor, RecipeObject recipeObject, List<ItemRecipeObject> itemRecipes, Dictionary<TransmutableItemState, TransmutableRecipeObject> transmutableRecipes)
        {
            RecipeType recipeType = recipeProcessor.RecipeType;
            if (recipeObject is TransmutableRecipeObject transmutableRecipeObject)
            {
                if (transmutableRecipes.ContainsKey(transmutableRecipeObject.InputState))
                {

                    Debug.LogWarning($"{recipeProcessor.name} has duplicate transmutable recipe for state {transmutableRecipeObject.InputState}");
                }
                transmutableRecipes[transmutableRecipeObject.InputState] = transmutableRecipeObject;
                return;
            }
            switch (recipeType)
            {
                case RecipeType.Item:
                    if (recipeObject is not ItemRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.Passive:
                    if (recipeObject is not PassiveItemRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.Generator:
                    if (recipeObject is not GeneratorItemRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                case RecipeType.Machine:
                    if (recipeObject is not ItemEnergyRecipeObject)
                    {
                        LogInvalidRecipeWarning(recipeType, recipeProcessor, recipeObject);
                        return;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);

            }
            itemRecipes.Add((ItemRecipeObject)recipeObject);
        }

        private static void LogInvalidRecipeWarning(RecipeType recipeType, RecipeProcessor recipeProcessor,
            RecipeObject recipeObject)
        {
            Debug.LogWarning($"Invalid recipe '{recipeObject.name}' not of type '{recipeType} Recipe' in recipe processor {recipeProcessor.name}");
        }
        
        public static ulong HashItemInputs(List<ItemSlot> inputs, HashSet<string> included = null)
        {
            ulong hash = 0;
            included ??= new HashSet<string>();
            for (int i = 0; i < inputs.Count; i++)
            {
                ItemSlot itemSlot = inputs[i];
                if (ReferenceEquals(itemSlot?.itemObject,null) || included.Contains(itemSlot.itemObject.id))
                {
                    continue;
                }
                included.Add(itemSlot.itemObject.id);
                ulong sum = 0;
                foreach (char c in itemSlot.itemObject.id)
                {
                    sum += 13*(ulong)c;
                }
                hash += sum * 17;
                
            }
            return hash;
        }
        
        public static ulong HashItemInputs(List<ItemSlot> first, List<ItemSlot> second)
        {
            HashSet<string> included = new HashSet<string>();
            ulong hash = 0;
            hash += HashItemInputs(first, included);
            hash += HashItemInputs(second, included);
            return hash;
        }
    }
}

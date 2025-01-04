using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Item.Slot;
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
    

        public static bool OutputsUsed(ItemRecipe itemRecipe)
        {
            return ItemSlotUtils.IsEmpty(itemRecipe.SolidOutputs) && ItemSlotUtils.IsEmpty(itemRecipe.FluidOutputs);
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
                case RecipeType.Burner:
                    if (recipeObject is not BurnerRecipeObject)
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
                if (ItemSlotUtils.IsItemSlotNull(itemSlot) || !included.Add(itemSlot.itemObject.id)) continue;
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

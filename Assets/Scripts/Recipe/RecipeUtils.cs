using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Item.Slot;
using Item.Transmutation;
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

    public static T TryCraftRecipe<T>(ItemRecipeObjectInstance[] recipeObjects,List<ItemSlot> solids, List<ItemSlot> fluids, RecipeType recipeType)
        where T : ItemRecipe
    {
        foreach (ItemRecipeObjectInstance itemRecipeInstance in recipeObjects)
        {
            if (TryConsumeItemRecipe(itemRecipeInstance, solids, fluids))
            {
                return (T)RecipeFactory.CreateRecipe(recipeType, itemRecipeInstance.ItemRecipeObject);
            }
        }
        return default;
    }
    public static T TryCraftRecipe<T>(ItemRecipeObjectInstance candidateRecipe, List<ItemSlot> solids,
        List<ItemSlot> fluids,  RecipeType recipeType) where T : ItemRecipe
    {
        if (TryConsumeItemRecipe(candidateRecipe, solids, fluids))
        {
            return (T)RecipeFactory.CreateRecipe(recipeType, candidateRecipe.ItemRecipeObject);
        }

        return default;
    }

    public static T TryCraftTransmutableRecipe<T>(TransmutableRecipeObject transmutableRecipe, ItemSlot inputItem, TransmutableItemMaterial material, RecipeType recipeType)
    where T : ItemRecipe
    {
        float efficency = transmutableRecipe.Efficency.Value();
        uint requiredAmount = (uint)TransmutableItemUtils.GetTransmutationRatio(transmutableRecipe.OutputState, transmutableRecipe.InputState, efficency);
        if (inputItem.amount < requiredAmount) return default;
        inputItem.amount -= requiredAmount;
        var output = TransmutableItemUtils.TransmuteOutput(material, transmutableRecipe.InputState, transmutableRecipe.OutputState);
        return (T)RecipeFactory.GetTransmutationRecipe(recipeType,material, transmutableRecipe.OutputState, output);
    }
    
    public static Dictionary<string, long> GetRequiredAmounts(ItemRecipeObjectInstance candidateRecipe)
    {
        var requiredItemAmounts = new Dictionary<string, long>();
        foreach (ItemSlot itemSlot in candidateRecipe.Inputs)
        {
            if (ReferenceEquals(itemSlot?.itemObject,null)) continue;
            if (requiredItemAmounts.ContainsKey(itemSlot.itemObject.id))
            {
                requiredItemAmounts[itemSlot.itemObject.id] += itemSlot.amount;
            }
            else
            {
                requiredItemAmounts[itemSlot.itemObject.id] = itemSlot.amount;
            }
        }

        return requiredItemAmounts;
    }
    public static bool TryConsumeItemRecipe(ItemRecipeObjectInstance candidateRecipe, List<ItemSlot> solids, List<ItemSlot> fluids)
    {
        var requiredItemAmounts = GetRequiredAmounts(candidateRecipe);

        DeiterateRequiredAmount(solids, requiredItemAmounts);
        DeiterateRequiredAmount(fluids, requiredItemAmounts);
            
        foreach (var kvp in requiredItemAmounts)
        {
            if (kvp.Value > 0) return false;
        }

        requiredItemAmounts = GetRequiredAmounts(candidateRecipe);
            
        DeiterateInputs(solids, requiredItemAmounts);
        DeiterateInputs(fluids, requiredItemAmounts);
        return true;
    }

    public static void DeiterateRequiredAmount(List<ItemSlot> inputs, Dictionary<string, long> requiredItemAmounts)
    {
        if (inputs == null) return;
        foreach (ItemSlot input in inputs)
        {
            if (ItemSlotUtils.IsItemSlotNull(input)) continue;
            if (!requiredItemAmounts.ContainsKey(input.itemObject.id)) continue;
            requiredItemAmounts[input.itemObject.id] -= input.amount;
        }
    }

    public static void DeiterateInputs(List<ItemSlot> inputs, Dictionary<string, long> requiredItemAmounts)
    {
        if (inputs == null) return;
        foreach (ItemSlot input in inputs)
        {
            if (ItemSlotUtils.IsItemSlotNull(input)) continue;
            if (!requiredItemAmounts.TryGetValue(input.itemObject.id, out var amount)) continue;
            uint requiredRemoval = (uint) amount;
            if (requiredRemoval == 0) continue;
            uint removal = requiredRemoval > Global.MAX_SIZE ? Global.MAX_SIZE : requiredRemoval;
            if (removal > input.amount) removal = input.amount;
            requiredRemoval -= removal;
            requiredItemAmounts[input.itemObject.id] = requiredRemoval;
            input.amount -= removal;
        }
    }

        public static bool OutputsUsed(ItemRecipe itemRecipe)
        {
            return ItemSlotUtils.IsEmpty(itemRecipe.SolidOutputs) && ItemSlotUtils.IsEmpty(itemRecipe.FluidOutputs);
        }
        
        
        public static void InsertValidRecipes(RecipeProcessor recipeProcessor, RecipeObject recipeObject, List<ItemRecipeObject> itemRecipes, Dictionary<TransmutableItemState, TransmutableRecipeObject> transmutableRecipes)
        {
            if (ReferenceEquals(recipeObject, null)) return;
            
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
            ItemRecipeObject itemRecipeObject = recipeObject as ItemRecipeObject;
            if (!itemRecipeObject)
            {
                Debug.LogWarning($"Null recipe in processor {recipeProcessor.name}");
                return;
            }

            foreach (EditorItemSlot editorItemSlot in itemRecipeObject.Inputs)
            {
                if (editorItemSlot.ItemObject) continue;
                Debug.LogWarning($"ItemRecipeObject {itemRecipeObject.name} is invalid");
                return;
            }

            foreach (RandomEditorItemSlot editorItemSlot in itemRecipeObject.Outputs)
            {
                if (editorItemSlot.ItemObject) continue;
                Debug.LogWarning($"ItemRecipeObject {itemRecipeObject.name} is invalid");
                return;
            }
            
            switch (recipeType)
            {
                case RecipeType.Item:
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
            if (inputs == null) return 0;
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
        public static bool CurrentValid(RecipeObject recipeObject, RecipeType recipeType)
        {
            if (!recipeObject) return false;
            switch (recipeType)
            {
                case RecipeType.Item:
                    return recipeObject is ItemRecipeObject;
                case RecipeType.Passive:
                    return recipeObject is PassiveItemRecipeObject;
                case RecipeType.Generator:
                    return recipeObject is GeneratorItemRecipeObject;
                case RecipeType.Machine:
                    return recipeObject is ItemEnergyRecipeObject;
                case RecipeType.Burner:
                    return recipeObject is BurnerRecipeObject;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
        public static ItemRecipeObject GetNewRecipeObject(RecipeType recipeType, RecipeObject template)
        {
            switch (recipeType)
            {
                case RecipeType.Item:
                    return ScriptableObject.CreateInstance<ItemRecipeObject>();
                case RecipeType.Passive:
                    if (template is PassiveItemRecipeObject)
                    {
                        return ScriptableObject.Instantiate(template) as ItemRecipeObject;
                    }
                    return ScriptableObject.CreateInstance<PassiveItemRecipeObject>();
                case RecipeType.Generator:
                    if (template is GeneratorItemRecipeObject)
                    {
                        return ScriptableObject.Instantiate(template) as GeneratorItemRecipeObject;
                    }
                    return ScriptableObject.CreateInstance<GeneratorItemRecipeObject>();
                case RecipeType.Machine:
                    if (template is ItemEnergyRecipeObject)
                    {
                        return ScriptableObject.Instantiate(template) as ItemEnergyRecipeObject;
                    }
                    return ScriptableObject.CreateInstance<ItemEnergyRecipeObject>();
                case RecipeType.Burner:
                    return ScriptableObject.CreateInstance<BurnerRecipeObject>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
    }
}

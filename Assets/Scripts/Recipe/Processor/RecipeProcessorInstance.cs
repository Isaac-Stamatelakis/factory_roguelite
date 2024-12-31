using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Transmutable;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Viewer;
using RecipeModule;
using TileEntity;
using UnityEngine;

namespace Recipe.Processor
{
    public class RecipeProcessorInstance
    {
        private RecipeProcessor recipeProcessorObject;
        public RecipeProcessor RecipeProcessorObject => recipeProcessorObject;
        private Dictionary<int, Dictionary<ulong, ItemRecipeObjectInstance[]>> modeRecipeDict;
        private Dictionary<string, List<DisplayableRecipe>> outputItemRecipes;
        private Dictionary<string, List<DisplayableRecipe>> inputItemRecipes;
        private Dictionary<int, Dictionary<TransmutableItemState, TransmutableRecipeObject>> modeRecipeTransmutation;
        public RecipeProcessorInstance(RecipeProcessor recipeProcessorObject)
        {
            this.recipeProcessorObject = recipeProcessorObject;
            InitializeModeRecipeDict();
        }
        
        private void InitializeModeRecipeDict()
        {
            modeRecipeDict = new Dictionary<int, Dictionary<ulong, ItemRecipeObjectInstance[]>>();
            modeRecipeTransmutation = new Dictionary<int, Dictionary<TransmutableItemState, TransmutableRecipeObject>>();
            int hashCollisions = 0;
            var tempModeRecipeDict = new Dictionary<int, Dictionary<ulong, List<ItemRecipeObject>>>();
            foreach (RecipeModeCollection recipeModeCollection in recipeProcessorObject.RecipeCollections)
            {
                int mode = recipeModeCollection.Mode;
                if (!tempModeRecipeDict.ContainsKey(mode))
                {
                    tempModeRecipeDict[mode] = new Dictionary<ulong, List<ItemRecipeObject>>();
                }

                if (!modeRecipeTransmutation.ContainsKey(mode))
                {
                    modeRecipeTransmutation[mode] = new Dictionary<TransmutableItemState, TransmutableRecipeObject>();
                }
                List<ItemRecipeObject> itemRecipes = new List<ItemRecipeObject>();
                foreach (RecipeObject recipeObject in recipeModeCollection.RecipeCollection.Recipes)
                {
                    RecipeUtils.InsertValidRecipes(recipeProcessorObject, recipeObject, itemRecipes,modeRecipeTransmutation[mode]);
                }
                
                foreach (ItemRecipeObject itemRecipeObject in itemRecipes)
                {
                    var inputs = new List<ItemSlot>();
                    foreach (EditorItemSlot editorItemSlot in itemRecipeObject.Inputs)
                    {
                        ItemSlot itemSlot = ItemSlotFactory.FromEditorObject(editorItemSlot);
                        if (!ReferenceEquals(itemSlot?.itemObject, null)) inputs.Add(itemSlot);
                    }
                    if (inputs.Count == 0) continue;
                    
                    ulong hash = RecipeUtils.HashItemInputs(inputs);
                    if (tempModeRecipeDict[mode].ContainsKey(hash))
                    {
                        hashCollisions++;
                    }
                    else
                    {
                        tempModeRecipeDict[mode][hash] = new List<ItemRecipeObject>();
                    }
                    tempModeRecipeDict[mode][hash].Add(itemRecipeObject);
                }
            }

            foreach (var kvp in tempModeRecipeDict)
            {
                if (kvp.Value.Count == 0) continue;
                int mode = kvp.Key;
                if (!modeRecipeDict.ContainsKey(mode))
                {
                    modeRecipeDict[mode] = new Dictionary<ulong, ItemRecipeObjectInstance[]>();
                }
                
                foreach (var hashDict in tempModeRecipeDict.Values)
                {
                    foreach (var hashKvp in hashDict)
                    {
                        var hash = hashKvp.Key;
                        var recipeObjectInstances = new ItemRecipeObjectInstance[hashKvp.Value.Count];
                        for (int i = 0; i < hashKvp.Value.Count; i++)
                        {
                            recipeObjectInstances[i] = new ItemRecipeObjectInstance(hashKvp.Value[i]);
                        }
                        modeRecipeDict[mode][hash] = recipeObjectInstances;
                    }
                }
            }
    
            List<int> emptyModes = new List<int>();
            foreach (var kvp in modeRecipeTransmutation)
            {
                if (kvp.Value.Count == 0) emptyModes.Add(kvp.Key);
            }
            foreach (int mode in emptyModes)
            {
                modeRecipeTransmutation.Remove(mode);
            }

            if (hashCollisions > 0) Debug.LogWarning($"RecipeProcessor '{RecipeProcessorObject.name} item recipe dict has {hashCollisions} hash collisions");
        }
        
        public T GetRecipe<T>(int mode, List<ItemSlot> solidItems, List<ItemSlot> fluidItems) where T : ItemRecipe
        {
            bool canTransmute = modeRecipeTransmutation.ContainsKey(mode);
            if (canTransmute)
            {
                var transmutableItems = new List<ItemSlot>();
                foreach (ItemSlot itemSlot in solidItems)
                {
                    if (itemSlot == null || itemSlot.itemObject == null ||
                        itemSlot.itemObject is not TransmutableItemObject transmutableItemObject)
                    {
                        continue;
                    }
                    transmutableItems.Add(itemSlot);
                }
                foreach (ItemSlot itemSlot in fluidItems)
                {
                    if (itemSlot == null || itemSlot.itemObject == null ||
                        itemSlot.itemObject is not TransmutableItemObject transmutableItemObject)
                    {
                        continue;
                    }
                    transmutableItems.Add(itemSlot);
                }
                Dictionary<TransmutableItemState, TransmutableRecipeObject> stateRecipeDict = modeRecipeTransmutation[mode];
                foreach (ItemSlot transmutableItem in transmutableItems)
                {
                    var transItemObject = transmutableItem.itemObject as TransmutableItemObject;
                    var state = transItemObject!.getState();
                    if (!stateRecipeDict.TryGetValue(state, out var transmutableRecipe)) continue;
                    float ratio = TransmutableItemUtils.GetTransmutationRatio(transmutableRecipe.InputState,transmutableRecipe.OutputState);
                    switch (transmutableRecipe.Efficency)
                    {
                        case TransmutationEfficency.Half:
                            ratio *= 2;
                            break;
                    }
                    uint requiredAmount = (uint)Mathf.CeilToInt(ratio);
                    if (transmutableItem.amount < requiredAmount)
                    {
                        continue;
                    }
                    transmutableItem.amount -= requiredAmount;
                    ItemSlot output = TransmutableItemUtils.Transmute(transItemObject.getMaterial(), transmutableRecipe.InputState, transmutableRecipe.OutputState);
                    return (T)RecipeFactory.GetTransmutationRecipe(recipeProcessorObject.RecipeType,transItemObject.getMaterial(), state, output);
                }
            }
            if (!modeRecipeDict.TryGetValue(mode, out var recipeDict))
            {
                return default;
            }
            ulong hash = RecipeUtils.HashItemInputs(solidItems,fluidItems);
            if (!recipeDict.TryGetValue(hash, out var recipeObjects))
            {
                return default;
            }
            foreach (ItemRecipeObjectInstance itemRecipeInstance in recipeObjects)
            {
                if (TryConsumeItemRecipe(itemRecipeInstance, solidItems, fluidItems))
                {
                    return (T)RecipeFactory.CreateRecipe(RecipeProcessorObject.RecipeType, itemRecipeInstance.ItemRecipeObject);
                }
            }
            return default;

        }
        

        private Dictionary<string, long> GetRequiredAmounts(ItemRecipeObjectInstance candiateRecipe)
        {
            var requiredItemAmounts = new Dictionary<string, long>();
            foreach (ItemSlot itemSlot in candiateRecipe.Inputs)
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

        private bool TryConsumeItemRecipe(ItemRecipeObjectInstance candiateRecipe, List<ItemSlot> solids, List<ItemSlot> fluids)
        {
            var requiredItemAmounts = GetRequiredAmounts(candiateRecipe);

            DeiterateRequiredAmount(solids, requiredItemAmounts);
            DeiterateRequiredAmount(fluids, requiredItemAmounts);
            
            foreach (var kvp in requiredItemAmounts)
            {
                if (kvp.Value > 0) return false;
            }

            requiredItemAmounts = GetRequiredAmounts(candiateRecipe);
            
            DeiterateInputs(solids, requiredItemAmounts);
            DeiterateInputs(fluids, requiredItemAmounts);
            return true;
        }

        private void DeiterateRequiredAmount(List<ItemSlot> inputs, Dictionary<string, long> requiredItemAmounts)
        {
            foreach (ItemSlot input in inputs)
            {
                if (ReferenceEquals(input?.itemObject,null)) continue;
                if (!requiredItemAmounts.ContainsKey(input.itemObject.id)) continue;
                requiredItemAmounts[input.itemObject.id] -= input.amount;
            }
        }

        private void DeiterateInputs(List<ItemSlot> inputs, Dictionary<string, long> requiredItemAmounts)
        {
            foreach (ItemSlot input in inputs)
            {
                if (ReferenceEquals(input?.itemObject,null)) continue;
                if (!requiredItemAmounts.ContainsKey(input.itemObject.id)) continue;
                uint requiredRemoval = (uint) requiredItemAmounts[input.itemObject.id];
                if (requiredRemoval == 0) continue;
                uint removal = requiredRemoval > Global.MaxSize ? Global.MaxSize : requiredRemoval;
                requiredRemoval -= removal;
                requiredItemAmounts[input.itemObject.id] = requiredRemoval;
                input.amount -= removal;
            }
        }
        public List<DisplayableRecipe> GetRecipesForItem(ItemSlot itemSlot)
        {
            if (itemSlot == null || itemSlot.itemObject == null)
            {
                return null;
            }
            string id = itemSlot.itemObject.id;
            return outputItemRecipes.GetValueOrDefault(id);
        }
        
        public List<DisplayableRecipe> GetRecipesWithItem(ItemSlot itemSlot)
        {
            if (itemSlot == null || itemSlot.itemObject == null)
            {
                return null;
            }
            string id = itemSlot.itemObject.id;
            return inputItemRecipes.GetValueOrDefault(id);
        }

        public List<DisplayableRecipe> GetAllRecipes()
        {
            List<DisplayableRecipe> recipes = new List<DisplayableRecipe>();
            foreach (var kvp in modeRecipeDict)
            {
                int mode = kvp.Key;
                foreach (var recipeKvp in kvp.Value)
                {
                    foreach (ItemRecipeObjectInstance recipe in recipeKvp.Value)
                    {
                        recipes.Add(new DisplayableRecipe(mode, recipe.ItemRecipeObject, this));
                    }
                }
            }

            foreach (var kvp in modeRecipeTransmutation)
            {
                int mode = kvp.Key;
                foreach (var transRecipe in kvp.Value)
                {
                    recipes.Add(new DisplayableRecipe(mode, transRecipe.Value, this));
                }
            }

            return recipes;
        }

        public int GetCount()
        {
            int count = 0;
            foreach (var kvp in modeRecipeDict)
            {
                count += kvp.Value.Count;
            }

            foreach (var kvp in modeRecipeTransmutation)
            {
                count += kvp.Value.Count;
            }

            return count;
        }
        
    }

    public static class RecipeFactory
    {
        public static ItemRecipe CreateRecipe(RecipeType recipeType, ItemRecipeObject recipeObject)
        {
            List<ItemSlot> outputCopy = ItemSlotFactory.FromEditorObjects(recipeObject.Outputs);
            ItemSlotUtils.sortInventoryByState(outputCopy,out var solidOutputs, out var fluidOutputs);
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(solidOutputs, fluidOutputs);
                case RecipeType.Passive:
                    PassiveItemRecipeObject passiveItemRecipeObject = (PassiveItemRecipeObject)recipeObject;
                    return new PassiveItemRecipe(solidOutputs,fluidOutputs, passiveItemRecipeObject.Ticks, passiveItemRecipeObject.Ticks);
                case RecipeType.Generator:
                    GeneratorItemRecipeObject generatorRecipeObject = (GeneratorItemRecipeObject)recipeObject;
                    return new GeneratorItemRecipe(solidOutputs,fluidOutputs, generatorRecipeObject.Ticks, generatorRecipeObject.Ticks, generatorRecipeObject.EnergyPerTick);
                case RecipeType.Machine:
                    ItemEnergyRecipeObject itemRecipeObject = (ItemEnergyRecipeObject)recipeObject;
                    return new ItemEnergyRecipe(solidOutputs,fluidOutputs, itemRecipeObject.TotalInputEnergy, itemRecipeObject.TotalInputEnergy, itemRecipeObject.MinimumEnergyPerTick);
                case RecipeType.Burner:
                    BurnerRecipeObject burnerRecipeObject = (BurnerRecipeObject)recipeObject;
                    return new BurnerItemRecipe(solidOutputs, fluidOutputs, burnerRecipeObject.Ticks, burnerRecipeObject.Ticks, burnerRecipeObject.PassiveSpeed);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
        public static ItemRecipe GetTransmutationRecipe(RecipeType recipeType, TransmutableItemMaterial material, TransmutableItemState outputState, ItemSlot output)
        {
            List<ItemSlot> solid = null;
            List<ItemSlot> fluid = null;
            ItemState itemState = outputState.getMatterState();
            switch (itemState)
            {
                case ItemState.Solid:
                    solid = new List<ItemSlot> { output };
                    break;
                case ItemState.Fluid:
                    fluid = new List<ItemSlot> { output };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(solid, fluid);
                case RecipeType.Machine:
                    ulong usage = TierUtils.GetMaxEnergyUsage(material.tier);
                    ulong cost = 32 * usage; // TODO change this
                    return new ItemEnergyRecipe(solid,fluid, cost, cost,usage);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Items.Transmutable;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Viewer;
using RecipeModule;
using TileEntity;
using Unity.VisualScripting;
using UnityEngine;

namespace Recipe.Processor
{
    public class RecipeProcessorInstance
    {
        private RecipeProcessor recipeProcessorObject;
        public RecipeProcessor RecipeProcessorObject => recipeProcessorObject;
        private Dictionary<int, List<RecipeObject>> modeRecipeObjects;
        private Dictionary<int, Dictionary<ulong, ItemRecipeObject>> modeRecipeDict;
        private Dictionary<ulong, List<DisplayableRecipe>> outputItemRecipes;
        private Dictionary<ulong, List<DisplayableRecipe>> inputItemRecipes;
        private Dictionary<int, List<TransmutableRecipeObject>> modeRecipeTransmutation;

        public RecipeProcessorInstance(RecipeProcessor recipeProcessorObject)
        {
            this.recipeProcessorObject = recipeProcessorObject;
            InitializeModeRecipeDict();
        }
        
        private void InitializeModeRecipeDict()
        {
            modeRecipeObjects = new Dictionary<int, List<RecipeObject>>();
            modeRecipeDict = new Dictionary<int, Dictionary<ulong, ItemRecipeObject>>();
            foreach (RecipeModeCollection recipeModeCollection in recipeProcessorObject.RecipeCollections)
            {
                int mode = recipeModeCollection.Mode;
                if (!modeRecipeObjects.ContainsKey(mode))
                {
                    modeRecipeObjects[mode] = new List<RecipeObject>();
                }
                foreach (RecipeObject recipeObject in recipeModeCollection.RecipeCollection.Recipes)
                {
                    RecipeUtils.InsertValidRecipes(recipeProcessorObject, recipeObject, modeRecipeObjects[mode]);
                }
            }
        }

        public T GetRecipe<T>(int mode, List<ItemSlot> inputs) where T : ItemRecipe
        {
            bool canTransmute = modeRecipeTransmutation != null;
            if (canTransmute && modeRecipeTransmutation.ContainsKey(mode))
            {
                T transmutationRecipe = GetTransmutationRecipe<T>(mode, inputs);
                if (transmutationRecipe != default(T))
                {
                    return transmutationRecipe;
                }
            }

            if (!modeRecipeDict.TryGetValue(mode, out var recipeDict))
            {
                return default;
            }

            ulong hash = RecipeUtils.HashItemInputs(inputs);
            if (!recipeDict.TryGetValue(hash, out ItemRecipeObject recipeObject))
            {
                return default;
            }
            return (T)RecipeFactory.GetRecipe(recipeProcessorObject.RecipeType, recipeObject);
        }
        
        public T GetRecipe<T>(int mode, List<ItemSlot> first, List<ItemSlot> second) where T : ItemRecipe
        {
            bool canTransmute = modeRecipeTransmutation != null;
            if (canTransmute && modeRecipeTransmutation.ContainsKey(mode))
            {
                T transmutationRecipe = GetTransmutationRecipe<T>(mode, first);
                if (transmutationRecipe != default(T))
                {
                    return transmutationRecipe;
                }
                transmutationRecipe = GetTransmutationRecipe<T>(mode, second);
                if (transmutationRecipe != default(T))
                {
                    return transmutationRecipe;
                }
            }

            if (!modeRecipeDict.TryGetValue(mode, out var recipeDict))
            {
                return default;
            }

            ulong hash = RecipeUtils.HashItemInputs(first,second);
            if (!recipeDict.TryGetValue(hash, out ItemRecipeObject recipeObject))
            {
                return default;
            }
            return (T)RecipeFactory.GetRecipe(recipeProcessorObject.RecipeType, recipeObject);
        }

        private T GetTransmutationRecipe<T>(int mode, List<ItemSlot> inputs)  where T : ItemRecipe
        {
            List<TransmutableRecipeObject> recipes = modeRecipeTransmutation[mode];
            foreach (TransmutableRecipeObject transmutableRecipeObject in recipes)
            {
                foreach (ItemSlot itemSlot in inputs)
                {
                    if (itemSlot == null || itemSlot.itemObject == null || itemSlot.itemObject is not TransmutableItemObject transmutableItemObject)
                    {
                        continue;
                    }
                    TransmutableItemState itemState = transmutableItemObject.getState();
                    if (itemState != transmutableRecipeObject.InputState)
                    {
                        continue;
                    }
                    ItemSlot output = TransmutableItemUtils.Transmute(transmutableItemObject.getMaterial(), transmutableRecipeObject.InputState, transmutableRecipeObject.OutputState);
                    return (T)RecipeFactory.GetTransmutationRecipe(recipeProcessorObject.RecipeType,transmutableItemObject.getMaterial(), itemState, output);
                }
            }
            
            return default;
        }

        public List<DisplayableRecipe> GetRecipesForItem(ItemSlot itemSlot)
        {
            ulong hash = RecipeUtils.HashItemInput(itemSlot);
            return outputItemRecipes.GetValueOrDefault(hash);
        }
        
        public List<DisplayableRecipe> GetRecipesWithItem(ItemSlot itemSlot)
        {
            ulong hash = RecipeUtils.HashItemInput(itemSlot);
            return inputItemRecipes.GetValueOrDefault(hash);
        }

        public List<DisplayableRecipe> GetAllRecipes()
        {
            List<DisplayableRecipe> recipes = new List<DisplayableRecipe>();
            foreach (var kvp in modeRecipeDict)
            {
                int mode = kvp.Key;
                foreach (var recipeKvp in kvp.Value)
                {
                    recipes.Add(new DisplayableRecipe(mode, recipeKvp.Value, this));
                }
            }

            foreach (var kvp in modeRecipeTransmutation)
            {
                int mode = kvp.Key;
                foreach (var transRecipe in kvp.Value)
                {
                    recipes.Add(new DisplayableRecipe(mode, transRecipe, this));
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

            return count;
        }
        
    }

    public static class RecipeFactory
    {
        public static ItemRecipe GetRecipe(RecipeType recipeType, ItemRecipeObject recipeObject)
        {
            List<ItemSlot> outputCopy = ItemSlotFactory.FromEditorObjects(recipeObject.Outputs);
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(outputCopy);
                case RecipeType.PassiveItem:
                    return new PassiveItemRecipe(outputCopy, ((PassiveItemRecipeObject)recipeObject).Ticks);
                case RecipeType.Generator:
                    GeneratorItemRecipeObject generatorRecipeObject = (GeneratorItemRecipeObject)recipeObject;
                    return new GeneratorItemRecipe(outputCopy, generatorRecipeObject.Ticks, generatorRecipeObject.EnergyPerTick);
                case RecipeType.EnergyItem:
                    ItemEnergyRecipeObject itemRecipeObject = (ItemEnergyRecipeObject)recipeObject;
                    return new ItemEnergyRecipe(outputCopy, itemRecipeObject.TotalInputEnergy,
                        itemRecipeObject.MinimumEnergyPerTick);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
        public static ItemRecipe GetTransmutationRecipe(RecipeType recipeType, TransmutableItemMaterial material, TransmutableItemState outputState, ItemSlot output)
        {
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new ItemRecipe(new List<ItemSlot>{ output });
                case RecipeType.EnergyItem:
                    ulong energyCost = TierUtils.GetMaxEnergyUsage(material.tier);
                    ulong ticks = 200; // TODO change this
                    return new ItemEnergyRecipe(new List<ItemSlot>{ output }, energyCost, ticks);
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
    }
}

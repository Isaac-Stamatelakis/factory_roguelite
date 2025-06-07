using System;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Items.Transmutable;
using Player;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using TileEntity;
using TileEntity.Instances.WorkBenchs;
using UI;
using UI.Catalogue.InfoViewer;
using UnityEngine;

namespace Recipe.Viewer {
    public static class RecipeViewerHelper
    {
        public static List<CatalogueElementData> GetRecipesWithItem(ItemSlot itemSlot) {
            RecipeRegistry recipeRegistry = RecipeRegistry.GetInstance();
            var recipesWithItemInInput = recipeRegistry.GetRecipesWithItemInInput(itemSlot);
            if (itemSlot.itemObject is TileItem { tileEntity: IProcessorTileEntity tileEntityProcessor }) { // If is processor, show recipes it makes
                RecipeProcessor processor = tileEntityProcessor.GetRecipeProcessor();
                recipesWithItemInInput[processor] = recipeRegistry.GetRecipeProcessorRecipes(processor);
            }
            List<CatalogueElementData> elements = FormatProcessorInfo(recipesWithItemInInput);
            return elements;
        }
        
        public static List<CatalogueElementData> GetRecipesForItem(ItemSlot itemSlot) {
            RecipeRegistry recipeRegistry = RecipeRegistry.GetInstance();
            var recipesWithOutput = recipeRegistry.GetRecipesWithItemInOutput(itemSlot);
            List<CatalogueElementData> elements = FormatProcessorInfo(recipesWithOutput);
            return elements;
        }
        
        private static List<CatalogueElementData> FormatProcessorInfo(Dictionary<RecipeProcessor, List<DisplayableRecipe>> dict)
        {
            List<CatalogueElementData> elements = new List<CatalogueElementData>();
            foreach (var (recipeProcessor, recipes) in dict)
            {
                if (recipes.Count == 0) continue;
                RecipeProcessorInstance instance = RecipeRegistry.GetProcessorInstance(recipeProcessor);
                elements.Add(new CatalogueElementData(new RecipeProcessorDisplayInfo(instance,recipes),CatalogueInfoDisplayType.Recipe));
            }
            return elements;
        }
        
        

        public static List<string> GetCostStringsFromItemDisplayable(ItemDisplayableRecipe displayableRecipe, RecipeType recipeType)
        {
            RecipeObject recipeObject = displayableRecipe.RecipeData.Recipe;
            switch (recipeType)
            {
                case RecipeType.Item:
                    return new List<string>();
                case RecipeType.Passive:
                    if (recipeObject is PassiveItemRecipeObject passiveRecipe)
                        return new List<string>
                        {
                            $"Duration: {passiveRecipe.Seconds:F1} s",
                        };
                    Debug.LogWarning("Passive item recipe object is not a PassiveItemRecipeObject");
                    return null;
                case RecipeType.Generator:
                    if (recipeObject is GeneratorItemRecipeObject generatorRecipe)
                    {
                        uint ticks = GlobalHelper.TileEntitySecondsToTicks(generatorRecipe.Seconds);
                        return new List<string>
                        {
                            $"Total Generation:{ ticks * generatorRecipe.EnergyPerTick} J",
                            $"Generation: {generatorRecipe.EnergyPerTick * Global.TicksPerSecond} J/s",
                            $"Duration: {generatorRecipe.Seconds} s",
                        };
                    }
                        
                    Debug.LogWarning("Passive item recipe object is not a Generator Recipe");
                    return null;
                case RecipeType.Machine:
                    if (recipeObject is ItemEnergyRecipeObject itemEnergyRecipe)
                        return new List<string>
                        {
                            $"Total Usage: {itemEnergyRecipe.TotalInputEnergy} J",
                            $"Usage: {itemEnergyRecipe.MinimumEnergyPerTick * Global.TicksPerSecond} J/s",
                            $"Duration: {(double)itemEnergyRecipe.TotalInputEnergy / itemEnergyRecipe.MinimumEnergyPerTick:F1} s",
                        };
                    if (recipeObject is TransmutableRecipeObject)
                    {
                        Tier tier = displayableRecipe.Tier;
                        ulong tickUsage = tier.GetMaxEnergyUsage();
                        ulong secondUsage = tickUsage * Global.TicksPerSecond;
                        ulong cost = 32 * tickUsage;
                        return new List<string>
                        {
                            $"Total Usage: {cost} J",
                            $"Usage: {secondUsage} J/s",
                            $"Duration: {(double) cost /secondUsage:F1} s",
                            $"Tier: {tier}",
                        };
                    }
                    Debug.LogWarning("Passive item recipe object is not a PassiveItemRecipeObject");
                    return null;
                case RecipeType.Burner:
                     if (recipeObject is BurnerRecipeObject burnerRecipeObject)
                        return new List<string>
                        {
                            $"Duration: {burnerRecipeObject.Seconds:F1} s",
                        };
                     if (recipeObject is TransmutableRecipeObject) {
                         Tier tier = displayableRecipe.Tier;
                         uint ticks = 50 * ((uint)tier + 2);
                         return new List<string>
                         {
                             $"Duration: {ticks / Global.TicksPerSecond} s",
                             $"Tier: {tier}",
                         };
                     }
                     return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
    }
}


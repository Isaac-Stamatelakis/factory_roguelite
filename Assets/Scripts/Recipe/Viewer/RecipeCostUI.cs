using System;
using System.Collections.Generic;
using Recipe.Data;
using Recipe.Objects;
using TMPro;
using UnityEngine;

namespace Recipe.Viewer
{
    public class RecipeCostUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textPrefab;

        public void Display(RecipeObject recipeObject, RecipeType recipeType)
        {
            GlobalHelper.deleteAllChildren(transform);
            List<string> costStrings = GetCostStrings(recipeObject, recipeType);
            foreach (string costString in costStrings)
            {
                TextMeshProUGUI costText = Instantiate(textPrefab, transform);
                costText.text = costString;
            }
        }

        private List<string> GetCostStrings(RecipeObject recipeObject, RecipeType recipeType)
        {
            switch (recipeType)
            {
                case RecipeType.Item:
                    return null;
                case RecipeType.Passive:
                    if (recipeObject is PassiveItemRecipeObject passiveRecipe)
                        return new List<string>
                        {
                            $"Time:{passiveRecipe.Ticks/50f:F2} Secs",
                        };
                    Debug.LogWarning("Passive item recipe object is not a PassiveItemRecipeObject");
                    return null;
                case RecipeType.Generator:
                    if (recipeObject is GeneratorItemRecipeObject generatorRecipe)
                        return new List<string>
                        {
                            $"Total:{(ulong)generatorRecipe.Ticks * generatorRecipe.EnergyPerTick}",
                            $"J/t:{generatorRecipe.EnergyPerTick}",
                            $"Time:{generatorRecipe.Ticks/50f:F2} Secs",
                            
                        };
                    Debug.LogWarning("Passive item recipe object is not a PassiveItemRecipeObject");
                    return null;
                case RecipeType.Machine:
                    if (recipeObject is ItemEnergyRecipeObject itemEnergyRecipe)
                        return new List<string>
                        {
                            $"Total:{itemEnergyRecipe.TotalInputEnergy}",
                            $"J/t:{itemEnergyRecipe.MinimumEnergyPerTick}",
                            $"Time:{(double)itemEnergyRecipe.TotalInputEnergy/itemEnergyRecipe.MinimumEnergyPerTick:F2} Secs",
                        };
                    Debug.LogWarning("Passive item recipe object is not a PassiveItemRecipeObject");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
            }
        }
    }
}

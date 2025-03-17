using System;
using System.Collections.Generic;
using System.IO;
using Items;
using Items.Transmutable;
using Recipe;
using Recipe.Objects;
using Recipe.Objects.Generation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public static class RecipeGeneratorUtils
{
    public static void GenerateRecipes(RecipeGenerator recipeGenerator, bool log)
    {
        bool error = false;
        if (!recipeGenerator.RecipeCollection)
        {
            error = true;
            if (log) Debug.LogError($"Recipe Generator {recipeGenerator.name} has no recipe collection");
        }

        if (log && recipeGenerator.InputAmounts.Count == 0)
        {
            Debug.LogWarning($"Recipe Generator {recipeGenerator.name} has no input amounts");
        }
        if (log && recipeGenerator.OutputAmounts.Count == 0)
        {
            Debug.LogWarning($"Recipe Generator {recipeGenerator.name} has no output amounts");
        }
        if (error) return;
        if (log && recipeGenerator.Inputs.Count != recipeGenerator.Outputs.Count)
        {
            Debug.LogWarning($"Recipe Generator {recipeGenerator.name} output count does not match input count");
        }

        string assetPath = AssetDatabase.GetAssetPath(recipeGenerator).Replace(".asset","");
        string parentPath = Path.GetDirectoryName(assetPath);
        string folderPath = recipeGenerator.name + "Gen";
        string generatedRecipePath = Path.Combine(parentPath, folderPath);
        if (!Directory.Exists(generatedRecipePath))
        {
            AssetDatabase.CreateFolder(parentPath, folderPath);
            AssetDatabase.Refresh();
            if (log) Debug.Log($"Created content folder for Recipe Generator {recipeGenerator.name}");
        }
        int iterations = Mathf.Min(recipeGenerator.Inputs.Count, recipeGenerator.Outputs.Count);
        int currentMultiplier = 1;
        for (int i = 0; i < iterations; i++)
        {
            if (recipeGenerator.Multiplier > 0)
            {
                currentMultiplier *= recipeGenerator.Multiplier;
            }

            bool recipeExists = i < recipeGenerator.GeneratedRecipes.Count;
            bool createNew = !recipeExists;
            if (recipeExists)
            {
                bool currentValid = CurrentValid(recipeGenerator.GeneratedRecipes[i],recipeGenerator.RecipeType);
                if (!currentValid && recipeGenerator.GeneratedRecipes[i])
                {
                    createNew = true;
                    DeleteRecipe(recipeGenerator, recipeGenerator.GeneratedRecipes[i]);
                    recipeGenerator.GeneratedRecipes.RemoveAt(i);
                }
            }
            
            ItemRecipeObject recipeObject = !createNew 
                ? recipeGenerator.GeneratedRecipes[i] 
                : GenerateRecipe(recipeGenerator, i, log, generatedRecipePath + "/",currentMultiplier);;
            
            SetUpRecipe(recipeObject, recipeGenerator,i,log,currentMultiplier);
            if (!createNew) continue;
            recipeGenerator.RecipeCollection.Recipes.Add(recipeObject);
            recipeGenerator.GeneratedRecipes.Add(recipeObject);

        }
    }

    private static bool CurrentValid(RecipeObject recipeObject, RecipeType recipeType)
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

    public static void DeleteRecipes(RecipeGenerator recipeGenerator)
    {
        foreach (var recipeObject in recipeGenerator.GeneratedRecipes)
        {
            DeleteRecipe(recipeGenerator, recipeObject);
        }

        recipeGenerator.GeneratedRecipes.Clear();
    }

    private static void DeleteRecipe(RecipeGenerator recipeGenerator, RecipeObject recipeObject)
    {
        if (!recipeObject) return;
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(recipeObject));
        if (!recipeGenerator.RecipeCollection) return;
        int index = recipeGenerator.RecipeCollection.Recipes.IndexOf(recipeObject);
        if (index == -1) return;
        recipeGenerator.RecipeCollection.Recipes.RemoveAt(index);
    }

    private static void SetUpRecipe(ItemRecipeObject recipeObject, RecipeGenerator recipeGenerator, int index, bool log, int multiplier)
    {
        recipeObject.Inputs = new List<EditorItemSlot>();
        recipeObject.Outputs = new List<RandomEditorItemSlot>();
        List<RecipeGenerationInput> inputs = recipeGenerator.Inputs[index].Inputs;
        List<ItemObject> outputs = recipeGenerator.Outputs[index].Inputs;
        
        for (int i = 0; i < recipeGenerator.InputAmounts.Count; i++)
        {
            if (i >= inputs.Count)
            {
                if (log) Debug.LogWarning($"Recipe Generator {recipeGenerator.name} inputs are less than input amounts at index {index} ");
                break;
            }
            RecipeGenerationInput input = inputs[i];
            EditorItemSlot editorItemSlot = FromGenerationInput(input, (uint)recipeGenerator.InputAmounts[i]);
            if (!editorItemSlot.ItemObject) continue;
            recipeObject.Inputs.Add(editorItemSlot);
        }
        
        for (int i = 0; i < recipeGenerator.OutputAmounts.Count; i++)
        {
            if (i >= outputs.Count)
            {
                if (log) Debug.LogWarning($"Recipe Generator {recipeGenerator.name} outputs are less than outputs amounts at index {index} ");
                break;
            }

            ItemObject output = outputs[i];
            RandomEditorItemSlot editorItemSlot = new RandomEditorItemSlot
            {
                Amount = (uint)recipeGenerator.OutputAmounts[i],
                ItemObject = output,
                Chance = 1f
            };
            if (!editorItemSlot.ItemObject) continue;
            recipeObject.Outputs.Add(editorItemSlot);
        }

        if (recipeObject is ItemEnergyRecipeObject itemEnergyRecipeObject)
        {
            itemEnergyRecipeObject.TotalInputEnergy *= (ulong)multiplier;
            itemEnergyRecipeObject.MinimumEnergyPerTick *= (ulong)multiplier;
        } if (recipeObject is GeneratorItemRecipeObject generatorItemRecipeObject)
        {
            generatorItemRecipeObject.EnergyPerTick *= (ulong)multiplier;
        } else if (recipeObject is PassiveItemRecipeObject passiveItemRecipeObject)
        {
            passiveItemRecipeObject.Ticks *= multiplier;
        }

    }

    private static ItemRecipeObject GenerateRecipe(RecipeGenerator recipeGenerator, int index, bool log, string saveFolder, int multiplier)
    {
        ItemRecipeObject recipeObject = GetNewRecipeObject(recipeGenerator.RecipeType,recipeGenerator.Template);
        if (!recipeObject)
        {
            if (log) Debug.LogError($"Could not generate recipe at index {index}");
            return null;
        }
        recipeObject.name = $"{recipeGenerator.name.Replace(".asset","")}_{index}";
        AssetDatabase.CreateAsset(recipeObject, saveFolder + recipeObject.name + ".asset");
        return recipeObject;
    }

    private static EditorItemSlot FromGenerationInput(RecipeGenerationInput input, uint amount)
    {
        switch (input.Mode)
        {
            case RecipeGenerationInputMode.Object:
                return new EditorItemSlot
                {
                    ItemObject = input.ItemObject,
                    Amount = amount,
                };
            case RecipeGenerationInputMode.Material:
                ItemObject itemObject = TransmutableItemGenerator.GetTransmutableItemObject(input.Material, input.ItemState);
                return new EditorItemSlot
                {
                    ItemObject = itemObject,
                    Amount = amount,
                };
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static ItemRecipeObject GetNewRecipeObject(RecipeType recipeType, RecipeObject template)
    {
        switch (recipeType)
        {
            case RecipeType.Item:
                return ScriptableObject.CreateInstance<ItemRecipeObject>();
            case RecipeType.Passive:
                if (template is PassiveItemRecipeObject)
                {
                    return Object.Instantiate(template) as ItemRecipeObject;
                }
                return ScriptableObject.CreateInstance<PassiveItemRecipeObject>();
            case RecipeType.Generator:
                if (template is GeneratorItemRecipeObject)
                {
                    return Object.Instantiate(template) as GeneratorItemRecipeObject;
                }
                return ScriptableObject.CreateInstance<GeneratorItemRecipeObject>();
            case RecipeType.Machine:
                if (template is ItemEnergyRecipeObject)
                {
                    return Object.Instantiate(template) as ItemEnergyRecipeObject;
                }
                return ScriptableObject.CreateInstance<ItemEnergyRecipeObject>();
            case RecipeType.Burner:
                return ScriptableObject.CreateInstance<BurnerRecipeObject>();
            default:
                throw new ArgumentOutOfRangeException(nameof(recipeType), recipeType, null);
        }
    }
}


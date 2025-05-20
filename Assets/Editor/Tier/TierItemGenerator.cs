using System;
using System.IO;
using Items;
using Recipe.Objects;
using Recipe.Processor;
using TileEntity;
using TileEntity.Instances;
using UnityEditor;
using UnityEngine;

namespace EditorScripts.Tier
{
    public abstract class TierItemGenerator
    {
        protected TierItemInfoObject tierItemInfoObject;
        protected TierItemGeneratorDefaults defaultValues;
        protected string generationPath;

        protected TierItemGenerator(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaultValues,
            string generationPath)
        {
            this.tierItemInfoObject = tierItemInfoObject;
            this.defaultValues = defaultValues;
            this.generationPath = generationPath;
        }

        public abstract void Generate();

        protected string TryCreateContentFolder(string folderName)
        {
            string contentPath = Path.Combine(generationPath, folderName);
            if (Directory.Exists(contentPath)) return contentPath;
            AssetDatabase.CreateFolder(generationPath, folderName);
            return contentPath;
        }

        protected T GetFirstObjectInFolder<T>(string folder) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { folder });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset) return asset;
            }

            return null;
        }

        protected void GetTierItemRecipes(string folder, out ItemRecipeObject workBenchRecipe, out ItemEnergyRecipeObject constructorRecipe)
        {
            workBenchRecipe = null;
            constructorRecipe = null;
            
            string[] guids = AssetDatabase.FindAssets("", new[] { folder });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                ItemRecipeObject asset = AssetDatabase.LoadAssetAtPath<ItemRecipeObject>(assetPath);
                if (asset is ItemEnergyRecipeObject itemEnergyRecipeObject)
                {
                    constructorRecipe = itemEnergyRecipeObject;
                    continue;
                }
                workBenchRecipe = asset;
            }
        }

        protected ItemGenerationData GenerateDefaultItemData(string itemName, ItemType itemType, RecipeGenerationMode recipeGenerationMode, int recipeMode = 0)
        {
            string folder = TryCreateContentFolder(itemName);
            ItemObject current;
            switch (itemType)
            {
                case ItemType.Crafting:
                    current = GetFirstObjectInFolder<ItemObject>(folder);
                    break;
                case ItemType.TileItem:
                    current = GetFirstObjectInFolder<TileItem>(folder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
            }
            
            if (!current)
            {
                current = ScriptableObject.CreateInstance<TileItem>();
                current.name = $"{ItemDisplayUtils.TIER_REPLACE_VALUE} Ladder";
                AssetDatabase.CreateAsset(current,Path.Combine(folder,current.name + ".asset"));
            }
            GetTierItemRecipes(folder, out ItemRecipeObject workBenchRecipe, out ItemEnergyRecipeObject constructorRecipe);
            
            switch (recipeGenerationMode)
            {
                case RecipeGenerationMode.None:
                    DeleteRecipe(recipeMode,defaultValues.RecipeProcessors.WorkBenchProcessor, workBenchRecipe);
                    DeleteRecipe(recipeMode,defaultValues.RecipeProcessors.ConstructorProcessor, constructorRecipe);
                    break;
                case RecipeGenerationMode.WorkBench:
                    DeleteRecipe(recipeMode,defaultValues.RecipeProcessors.ConstructorProcessor, constructorRecipe);
                    if (!workBenchRecipe) workBenchRecipe = CreateRecipe<ItemRecipeObject>("WorkBench");
                    break;
                case RecipeGenerationMode.Constructor:
                    DeleteRecipe(recipeMode,defaultValues.RecipeProcessors.WorkBenchProcessor, workBenchRecipe);
                    if (!constructorRecipe) constructorRecipe = CreateRecipe<ItemEnergyRecipeObject>("Constructor");
                    break;
                case RecipeGenerationMode.All:
                    if (!workBenchRecipe) workBenchRecipe = CreateRecipe<ItemRecipeObject>("WorkBench");
                    if (!constructorRecipe) constructorRecipe = CreateRecipe<ItemEnergyRecipeObject>("Constructor");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(recipeGenerationMode), recipeGenerationMode, null);
            }

            return new ItemGenerationData
            {
                ItemObject = current,
                WorkBenchRecipeObject = workBenchRecipe,
                ConstructorRecipeObject = constructorRecipe
            };

            T CreateRecipe<T>(string processorName) where T : ScriptableObject
            {
                T recipe = ScriptableObject.CreateInstance<T>();
                recipe.name = $"{itemName}_{processorName}";
                AssetDatabase.CreateAsset(recipe,Path.Combine(folder,recipe.name + ".asset"));
                return recipe;
            }
            
            void DeleteRecipe(int mode, RecipeProcessor recipeProcessor, RecipeObject recipeObject)
            {
                if (!recipeObject) return;
                recipeProcessor.RemoveRecipe(mode,recipeObject);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(recipeObject));
            }
        }

        

        protected TileEntityItemGenerationData GenerateDefaultTileEntityItemData<T>(string itemName, RecipeGenerationMode recipeGenerationMode, int recipeMode = 0) where T : TileEntityObject
        {
            ItemGenerationData itemGenerationData = GenerateDefaultItemData(itemName, ItemType.TileItem, recipeGenerationMode,recipeMode);
            string folder = Path.Combine(generationPath, itemName);
            T tileEntityObject = GetFirstObjectInFolder<T>(folder);
            if (!tileEntityObject)
            {
                string tileEntityName = typeof(T).Name;
                tileEntityObject = ScriptableObject.CreateInstance<T>();
                tileEntityObject.name = $"T~{tileEntityName}";
                AssetDatabase.CreateAsset(tileEntityObject, Path.Combine(folder,tileEntityObject.name + ".asset"));
            }

            return new TileEntityItemGenerationData
            {
                ItemGenerationData = itemGenerationData,
                TileEntityObject = tileEntityObject
            };
        }


        protected enum RecipeGenerationMode
        {
            None = 0,
            WorkBench = 1,
            Constructor = 2,
            All = 3,
        }

        protected enum ItemType
        {
            Crafting,
            TileItem
        }

        protected class ItemGenerationData
        {
            public ItemObject ItemObject;
            public ItemRecipeObject WorkBenchRecipeObject;
            public RecipeObject ConstructorRecipeObject;
        }
        protected class TileEntityItemGenerationData
        {
            public TileEntityObject TileEntityObject;
            public ItemGenerationData ItemGenerationData;
        }
    }
}

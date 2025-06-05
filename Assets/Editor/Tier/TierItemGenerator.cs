using System;
using System.Collections.Generic;
using System.IO;
using Item.GameStage;
using Item.Slot;
using Item.Transmutation;
using Items;
using Items.Transmutable;
using Recipe;
using Recipe.Objects;
using Recipe.Processor;
using Tier.Generators.Defaults;
using TileEntity;
using TileEntity.Instances;
using UnityEditor;
using UnityEngine;

namespace EditorScripts.Tier
{
    public interface ITierGeneratedItemData
    {
        public void SaveAssets();
    }
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

        protected abstract ITierGeneratedItemData GenerateItemData();

        public void Generate()
        {
            ITierGeneratedItemData itemData = GenerateItemData();
            itemData.SaveAssets();
        }
        
        
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
                if (!asset) continue;
                if (asset is ItemEnergyRecipeObject itemEnergyRecipeObject)
                {
                    constructorRecipe = itemEnergyRecipeObject;
                    continue;
                }
                workBenchRecipe = asset;
            }
        }

        protected ItemGenerationData GenerateDefaultItemData(TierGeneratedItemType tierGeneratedItemType, ItemType itemType, int recipeMode = 0, bool useTierName = false)
        {
            string itemName = GlobalHelper.AddSpaces(tierGeneratedItemType.ToString());
            string folder = TryCreateContentFolder(itemName);
            ItemObject current = itemType switch
            {
                ItemType.Crafting => GetFirstObjectInFolder<ItemObject>(folder),
                ItemType.TileItem => GetFirstObjectInFolder<TileItem>(folder),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };

            if (!current)
            {
                current = itemType switch
                {
                    ItemType.Crafting => ScriptableObject.CreateInstance<CraftingItem>(),
                    ItemType.TileItem => ScriptableObject.CreateInstance<TileItem>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
                };

                current.name = GetItemName();
                AssetDatabase.CreateAsset(current,Path.Combine(folder,current.name + ".asset"));
                string assetPath = AssetDatabase.GetAssetPath(current);
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                EditorUtils.AssignAddressablesLabel(guid,new List<AssetLabel> { AssetLabel.Item },AssetGroup.Items);
            }
            TileEntity.Tier tier = TileEntity.Tier.Basic;
            if (tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage) tier = tieredGameStage.Tier;

            RecipeGenerationMode recipeGenerationMode = tier > TileEntity.Tier.Master ? RecipeGenerationMode.Constructor : RecipeGenerationMode.All;
            current.name = GetItemName();
            current.id = current.name.ToLower().Replace(" ", "_");
            current.SetGameStageObject(tierItemInfoObject.GameStageObject);
            EditorUtility.SetDirty(current);
            AssetDatabase.SaveAssetIfDirty(current);
            
            GetTierItemRecipes(folder, out ItemRecipeObject workBenchRecipe, out ItemEnergyRecipeObject constructorRecipe);
            switch (recipeGenerationMode)
            {
                case RecipeGenerationMode.Constructor:
                    DeleteRecipe(recipeMode,defaultValues.RecipeProcessors.WorkBenchProcessor, workBenchRecipe);
                    if (!constructorRecipe) constructorRecipe = CreateRecipe<ItemEnergyRecipeObject>(defaultValues.RecipeProcessors.ConstructorProcessor);
                    break;
                case RecipeGenerationMode.All:
                    if (!workBenchRecipe) workBenchRecipe = CreateRecipe<ItemRecipeObject>(defaultValues.RecipeProcessors.WorkBenchProcessor);
                    if (!constructorRecipe) constructorRecipe = CreateRecipe<ItemEnergyRecipeObject>(defaultValues.RecipeProcessors.ConstructorProcessor);
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

            string GetItemName()
            {
                return useTierName 
                    ? $"{tierItemInfoObject.GameStageObject?.GetGameStageName()} {itemName}" 
                    : $"{tierItemInfoObject.PrimaryMaterial.name} {itemName}";
            }

            T CreateRecipe<T>(RecipeProcessor recipeProcessor) where T : RecipeObject
            {
                T recipe = ScriptableObject.CreateInstance<T>();
                recipe.name = $"{itemName}_{recipeProcessor.name}";
                AssetDatabase.CreateAsset(recipe,Path.Combine(folder,recipe.name + ".asset"));
                
                RecipeCollection recipeCollection = recipeProcessor.GetRecipeCollection(recipeMode);
                if (!recipeCollection)
                {
                    Debug.LogWarning($"Could not sync recipe for '{itemName}' in processor '{recipeProcessor.name}' with mode '{recipeMode}' as processor does not have a collection in mode");
                }
                else
                {
                    recipeCollection.Recipes.Add(recipe);
                    EditorUtility.SetDirty(recipeCollection);
                }

                EditorUtility.SetDirty(recipe);
                
                return recipe;
            }
            
            void DeleteRecipe(int mode, RecipeProcessor recipeProcessor, RecipeObject recipeObject)
            {
                if (!recipeObject) return;
                RecipeCollection recipeCollection = recipeProcessor.GetRecipeCollection(mode);
                if (recipeCollection)
                {
                    if (recipeCollection.Recipes.Contains(recipeObject))
                    {
                        EditorUtility.SetDirty(recipeCollection);
                        recipeCollection.Recipes.Remove(recipeObject);
                    }
                }
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(recipeObject));
            }
        }

        protected void AssignBasicItemRecipes(RandomEditorItemSlot output, List<EditorItemSlot> inputs, ulong ticks, ItemGenerationData itemGenerationData)
        {
            ApplyItemRecipe(itemGenerationData.WorkBenchRecipeObject);
            ApplyItemRecipe(itemGenerationData.ConstructorRecipeObject);

            if (!itemGenerationData.ConstructorRecipeObject) return;
            TileEntity.Tier tier = TileEntity.Tier.Basic;
            if (tierItemInfoObject.GameStageObject is TieredGameStage tieredGameStage)
            {
                tier = tieredGameStage.Tier;
            }
            ulong costPerTick = tier.GetMaxEnergyUsage();
            ulong energyCost = costPerTick * ticks;
            itemGenerationData.ConstructorRecipeObject.MinimumEnergyPerTick = costPerTick;
            itemGenerationData.ConstructorRecipeObject.TotalInputEnergy = energyCost;
            
            return;
            void ApplyItemRecipe(ItemRecipeObject itemRecipeObject)
            {
                if (!itemRecipeObject) return;
                itemRecipeObject.Outputs = new List<RandomEditorItemSlot> { output };
                itemRecipeObject.Inputs = inputs;
            }
        }
        

        protected TileEntityItemGenerationData GenerateDefaultTileEntityItemData<T>(TierGeneratedItemType tierGeneratedItemType, int recipeMode = 0, bool useTierName = false) where T : TileEntityObject
        {
            ItemGenerationData itemGenerationData = GenerateDefaultItemData(tierGeneratedItemType,ItemType.TileItem,recipeMode,useTierName:useTierName);
            string itemName = GlobalHelper.AddSpaces(tierGeneratedItemType.ToString());
            string folder = Path.Combine(generationPath, itemName);
            
            T tileEntityObject = GetFirstObjectInFolder<T>(folder);
            if (!tileEntityObject)
            {
                string tileEntityName = typeof(T).Name;
                tileEntityObject = ScriptableObject.CreateInstance<T>();
                tileEntityObject.name = $"T~{tileEntityName}";
                AssetDatabase.CreateAsset(tileEntityObject, Path.Combine(folder,tileEntityObject.name + ".asset"));
            }
            EditorUtility.SetDirty(tileEntityObject);

            TileItem tileItem = (TileItem)itemGenerationData.ItemObject;
            tileItem.tileEntity = tileEntityObject;

            return new TileEntityItemGenerationData
            {
                ItemGenerationData = itemGenerationData,
                TileEntityObject = tileEntityObject
            };
        }

        protected string GetItemFolder(TierGeneratedItemType tierGeneratedItemType)
        {
            string itemName = GlobalHelper.AddSpaces(tierGeneratedItemType.ToString());
            return Path.Combine(generationPath, itemName);
        }

        protected ItemObject LookUpGeneratedItem(TierGeneratedItemType tierGeneratedItemType)
        {
            string folder = GetItemFolder(tierGeneratedItemType);
            ItemObject first = GetFirstObjectInFolder<ItemObject>(folder);
            if (!first) Debug.LogWarning($"Could not find item {tierGeneratedItemType}. Is your generation order correct?");
            return first;
        }

        protected EditorItemSlot StateToItem(TransmutableItemState state, uint amount)
        {
            ItemObject itemObject = EditorUtils.GetTransmutableItemObject(tierItemInfoObject.PrimaryMaterial, state);
            return new EditorItemSlot(itemObject?.id, amount);
        }
        


        protected enum RecipeGenerationMode
        {
            Constructor = 1,
            All = 2,
        }

        protected enum ItemType
        {
            Crafting,
            TileItem
        }

        protected class ItemGenerationData : ITierGeneratedItemData
        {
            public ItemObject ItemObject;
            public ItemRecipeObject WorkBenchRecipeObject;
            public ItemEnergyRecipeObject ConstructorRecipeObject;

            public RandomEditorItemSlot ToRandomEditorSlot(uint amount, float chance = 1f)
            {
                return new RandomEditorItemSlot(ItemObject?.id, amount, chance);
            }

            public void SaveAssets()
            {
                EditorUtils.SaveAsset(ItemObject);
                EditorUtils.SaveAsset(WorkBenchRecipeObject);
                EditorUtils.SaveAsset(ConstructorRecipeObject);
            }
        }
        protected class TileEntityItemGenerationData : ITierGeneratedItemData
        {
            public TileEntityObject TileEntityObject;
            public ItemGenerationData ItemGenerationData;
            public void SaveAssets()
            {
                ItemGenerationData.SaveAssets();
                EditorUtils.SaveAsset(TileEntityObject);
            }
        }
    }
}

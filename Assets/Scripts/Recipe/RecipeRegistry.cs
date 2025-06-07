using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Items;
using Recipe.Collection;
using Recipe.Data;
using Recipe.Objects;
using Recipe.Processor;
using Recipe.Viewer;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using BurnableItemRegistry = Item.Registry.BurnableItemRegistry;

namespace Recipe {
    public class RecipeRegistry
    {
        private static Dictionary<RecipeProcessor, RecipeProcessorInstance> processorDict;
        private static List<RecipeProcessorInstance> processors;
        private static RecipeRegistry instance;
        private RecipeRegistry() {
            processors = new List<RecipeProcessorInstance>();
            processorDict = new Dictionary<RecipeProcessor, RecipeProcessorInstance>();
        }
        
        public static IEnumerator LoadRecipes() {
            if (instance != null) {
                yield break;
            }
            
            instance = new RecipeRegistry();
            
            var handle = Addressables.LoadAssetsAsync<RecipeProcessor>("recipe_processor", null);
            
            yield return handle;
            LoadProcessors(handle);
        }

        private static void LoadProcessors(AsyncOperationHandle<IList<RecipeProcessor>> processorHandles)
        {
            if (processorHandles.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load recipe processors from addressables: " + processorHandles.OperationException);
                return;
            }
            foreach (var asset in processorHandles.Result)
            {
                if (ReferenceEquals(asset,null)) continue;
                var processorInstance = new RecipeProcessorInstance(asset);
                processors.Add(processorInstance);
                processorDict[asset] = processorInstance;
            }
            int count = 0;
            foreach (RecipeProcessorInstance processorInstance in processors)
            {
                count += processorInstance.GetCount();
            }
            Debug.Log("Recipe Registry Initialized! Loaded " + processors.Count + " recipe processors & " + count + " recipes");
        }
        
        
        
        public static RecipeRegistry GetInstance() {
            if (instance == null) {
                throw new NullReferenceException("Tried to access null recipe registry");
            }
            return instance;
        }

        public static RecipeProcessorInstance GetProcessorInstance(RecipeProcessor processor)
        {
            RecipeProcessorInstance val = processorDict.GetValueOrDefault(processor);
            if (val == null)
            {
                Debug.LogWarning($"Could not find recipe processor instance for {processor.name} in registry. Did you forget to put in addressables?");
            }
            return val;
        }

        public List<RecipeProcessorInstance> GetAllProcessorsOfType(RecipeType recipeType)
        {
            List<RecipeProcessorInstance> typedProcessors = new List<RecipeProcessorInstance>();
            foreach (var processor in processors)
            {
                if (processor.RecipeProcessorObject.RecipeType != recipeType) continue;
                typedProcessors.Add(processor);
            }
            return typedProcessors;
        }

        public Dictionary<RecipeProcessor, List<DisplayableRecipe>> GetRecipesWithItemInOutput(ItemSlot itemSlot) {
            var processorOutput = new Dictionary<RecipeProcessor, List<DisplayableRecipe>>();
            foreach (RecipeProcessorInstance recipeProcessor in processors)
            {
                var recipes = recipeProcessor.GetRecipesForItem(itemSlot);
                if (recipes == null)
                {
                    continue;
                }
                processorOutput[recipeProcessor.RecipeProcessorObject] = recipes;
                
            }
            return processorOutput;
        }

        public Dictionary<RecipeProcessor, List<DisplayableRecipe>> GetRecipesWithItemInInput(ItemSlot itemSlot) {
            var processorRecipesWithItemInInput = new Dictionary<RecipeProcessor, List<DisplayableRecipe>>();
            foreach (RecipeProcessorInstance recipeProcessor in processors) {
                List<DisplayableRecipe> recipes = recipeProcessor.GetRecipesWithItem(itemSlot);
                if (recipes == null)
                {
                    continue;
                }
                processorRecipesWithItemInInput[recipeProcessor.RecipeProcessorObject] = recipes;
            }
            return processorRecipesWithItemInInput;
        }

        public List<DisplayableRecipe> GetRecipeProcessorRecipes(RecipeProcessor recipeProcessor)
        {
            return !processorDict.ContainsKey(recipeProcessor) ? null : processorDict[recipeProcessor].GetAllRecipesToDisplay();
        }

    }

}

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

namespace Recipe {
    public class RecipeRegistry
    {
        private static Dictionary<RecipeProcessor, RecipeProcessorInstance> processorDict;
        private static List<RecipeProcessorInstance> processors;
        private static BurnableItemRegistry burnableItemRegistry;
        public static BurnableItemRegistry BurnableItemRegistry => burnableItemRegistry;
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
            var burnableCollectionHandle = Addressables.LoadAssetAsync<BurnableItemCollection>("Assets/Objects/Recipe/BurnRegistry.asset");
            yield return handle;
            yield return burnableCollectionHandle;
            LoadProcessors(handle);
            LoadBurnableHandle(burnableCollectionHandle);
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
        

        private static void LoadBurnableHandle(AsyncOperationHandle<BurnableItemCollection> burnableHandle)
        {
            if (burnableHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load burnable recipe collections: " + burnableHandle.OperationException);
                return;
            }
            var collection = burnableHandle.Result;
            burnableItemRegistry = new BurnableItemRegistry(collection);
            Debug.Log($"Burnable item registry loaded with {burnableItemRegistry.MaterialCount} materials and {burnableItemRegistry.ItemCount} items");
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

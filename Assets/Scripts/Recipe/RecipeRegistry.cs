using System.Collections.Generic;
using Recipe.Data;
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
        private static RecipeRegistry instance;
        private RecipeRegistry() {
            processors = new List<RecipeProcessorInstance>();
            processorDict = new Dictionary<RecipeProcessor, RecipeProcessorInstance>();
            Addressables.LoadAssetsAsync<RecipeProcessor>("recipe_processor",null).Completed += OnProcessorsLoaded;
        }
        private static void OnProcessorsLoaded(AsyncOperationHandle<IList<RecipeProcessor>> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (var asset in handle.Result)
                {
                    var processorInstance = new RecipeProcessorInstance(asset);
                    processors.Add(processorInstance);
                    processorDict[asset] = processorInstance;
                }

                int count = 0;
                foreach (RecipeProcessorInstance processorInstance in processors)
                {
                    count += processorInstance.GetCount();
                }
                Debug.Log("Recipe registry loaded " + processors.Count + " recipe processors and " + count + " recipes");
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("Failed to load assets");
            }
        }
       
        public static RecipeRegistry GetInstance() {
            if (instance == null) {
                instance = new RecipeRegistry();
            }
            return instance;
        }

        public static RecipeProcessorInstance GetProcessorInstance(RecipeProcessor processor)
        {
            return processorDict.GetValueOrDefault(processor);
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
            return !processorDict.ContainsKey(recipeProcessor) ? null : processorDict[recipeProcessor].GetAllRecipes();
        }

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Items;

namespace RecipeModule {
    public class RecipeRegistry 
    {
        private static HashSet<RecipeProcessor> processors;
        private static RecipeRegistry instance;
        private RecipeRegistry() {
            int recipeCount = 0;
            processors = new HashSet<RecipeProcessor>();
            RecipeProcessor[] recipeProcessors = Resources.LoadAll<RecipeProcessor>("");
            foreach (RecipeProcessor recipeProcessor in recipeProcessors) {
                if (recipeProcessor is IInitableRecipeProcessor initableRecipeProcessor) {
                    initableRecipeProcessor.init();
                }
                if (recipeProcessor is not IRegisterableProcessor) {
                    continue;
                }
                processors.Add(recipeProcessor);
                
                if (recipeProcessor is IRecipeProcessor countable) {
                    recipeCount += countable.getRecipeCount();
                }
            }
            Debug.Log("Recipe registry loaded " + processors.Count + " recipe processors and " + recipeCount + " recipes");
        }
        public static RecipeRegistry getInstance() {
            if (instance == null) {
                instance = new RecipeRegistry();
            }
            return instance;
        }

        public Dictionary<RecipeProcessor, List<IRecipe>> getRecipesWithItemInOutput(ItemSlot itemSlot) {
            Dictionary<RecipeProcessor, List<IRecipe>> processorRecipesWithItemInOutput = new Dictionary<RecipeProcessor, List<IRecipe>>();
            ItemTagKey itemTagKey = new ItemTagKey(itemSlot.tags);
            foreach (RecipeProcessor recipeProcessor in processors) {
                List<IRecipe> haveInOutput = new List<IRecipe>();
                List<IRecipe> recipes = recipeProcessor.getRecipes();
                
                foreach (IRecipe recipe in recipes) {
                    List<ItemSlot> outputs = recipe.getOutputs();
                    foreach (ItemSlot outputItemSlot in outputs) {
                        if (outputItemSlot == null || outputItemSlot.itemObject == null) {
                            continue;
                        }
                        ItemTagKey outputItemTag = new ItemTagKey(outputItemSlot.tags);
                        if (outputItemSlot.itemObject.id == itemSlot.itemObject.id && itemTagKey.Equals(outputItemTag)) {
                            haveInOutput.Add(recipe);
                            break;
                        }
                    }
                }
                if (haveInOutput.Count == 0) {
                    continue;
                }
                processorRecipesWithItemInOutput[recipeProcessor] = haveInOutput;

            }
            return processorRecipesWithItemInOutput;
        }

        public Dictionary<RecipeProcessor, List<IRecipe>> getRecipesWithItemInInput(ItemSlot itemSlot) {
            Dictionary<RecipeProcessor, List<IRecipe>> processorRecipesWithItemInInput = new Dictionary<RecipeProcessor, List<IRecipe>>();
            ItemTagKey itemTagKey = new ItemTagKey(itemSlot.tags);
            foreach (RecipeProcessor recipeProcessor in processors) {
                List<IRecipe> haveInInput = new List<IRecipe>();
                List<IRecipe> recipes = recipeProcessor.getRecipes();

                foreach (IRecipe recipe in recipes) {
                    List<ItemSlot> inputs = recipe.getInputs();
                    foreach (ItemSlot inputItemSlot in inputs) {
                        if (inputItemSlot == null || inputItemSlot.itemObject == null) {
                            continue;
                        }
                        ItemTagKey inputTagKey = new ItemTagKey(inputItemSlot.tags);
                        if (inputItemSlot.itemObject.id == itemSlot.itemObject.id && itemTagKey.Equals(inputTagKey)) {
                            haveInInput.Add(recipe);
                            break;
                        }
                    }
                }
                if (haveInInput.Count == 0) {
                    continue;
                }
                processorRecipesWithItemInInput[recipeProcessor] = haveInInput;

            }
            return processorRecipesWithItemInInput;
        }

        public List<IRecipe> getRecipeProcessorRecipes(RecipeProcessor recipeProcessor) {
            if (!processors.Contains(recipeProcessor)) {
                return null;
            }
            return recipeProcessor.getRecipes();
        }

    }

}

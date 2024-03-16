using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

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

        private void loadItemList(List<ItemSlot> items, List<string> paths) {
            for (int i = 0; i < items.Count; i ++) {
                ItemSlot itemSlot = items[i];
                if (itemSlot.itemObject == null) {
                    if (i < paths.Count) {
                        itemSlot.itemObject = AssetDatabase.LoadAssetAtPath<ItemObject>(paths[i]);
                    } else {
                        Debug.LogError("Recipe Registry tried to load itemobject as it was null from path which did not exist");
                    }
                } else {
                    if (paths.Count <= i) {
                        paths.Add(AssetDatabase.GetAssetPath(itemSlot.itemObject));
                    } else {
                        paths[i] = AssetDatabase.GetAssetPath(itemSlot.itemObject);
                    } 
                }
            }
        }

        public static RecipeRegistry getInstance() {
            if (instance == null) {
                instance = new RecipeRegistry();
            }
            return instance;
        }

        public Dictionary<RecipeProcessor, List<Recipe>> getRecipesWithItemInOutput(ItemObject itemObject) {
            Dictionary<RecipeProcessor, List<Recipe>> processorRecipesWithItemInOutput = new Dictionary<RecipeProcessor, List<Recipe>>();
            foreach (RecipeProcessor recipeProcessor in processors) {
                List<Recipe> haveInOutput = new List<Recipe>();
                List<Recipe> recipes = recipeProcessor.getRecipes();

                foreach (Recipe recipe in recipes) {
                    List<ItemSlot> outputs = recipe.getOutputs();
                    foreach (ItemSlot outputItemSlot in outputs) {
                        if (outputItemSlot == null || outputItemSlot.itemObject == null) {
                            continue;
                        }
                        if (outputItemSlot.itemObject.id == itemObject.id) {
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

        public Dictionary<RecipeProcessor, List<Recipe>> getRecipesWithItemInInput(ItemObject itemObject) {
            Dictionary<RecipeProcessor, List<Recipe>> processorRecipesWithItemInInput = new Dictionary<RecipeProcessor, List<Recipe>>();
            foreach (RecipeProcessor recipeProcessor in processors) {
                List<Recipe> haveInInput = new List<Recipe>();
                List<Recipe> recipes = recipeProcessor.getRecipes();

                foreach (Recipe recipe in recipes) {
                    List<ItemSlot> inputs = recipe.getInputs();
                    foreach (ItemSlot inputItemSlot in inputs) {
                        if (inputItemSlot == null || inputItemSlot.itemObject == null) {
                            continue;
                        }
                        if (inputItemSlot.itemObject.id == itemObject.id) {
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

        public List<Recipe> getRecipeProcessorRecipes(RecipeProcessor recipeProcessor) {
            if (!processors.Contains(recipeProcessor)) {
                return null;
            }
            return recipeProcessor.getRecipes();
        }

    }

}

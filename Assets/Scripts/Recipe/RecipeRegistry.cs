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
                processors.Add(recipeProcessor);
                if (recipeProcessor is IInitableRecipeProcessor initableRecipeProcessor) {
                    initableRecipeProcessor.init();
                }
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
    }

}

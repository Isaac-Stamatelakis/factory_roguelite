using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace RecipeModule {
    public class RecipeRegistry 
    {
        private static Dictionary<string,RecipeProcessor> processors;
        private static RecipeRegistry instance;
        private RecipeRegistry() {
            int recipeCount = 0;
            processors = new Dictionary<string, RecipeProcessor>();
            RecipeProcessor[] recipeProcessors = Resources.LoadAll<RecipeProcessor>("");
            foreach (RecipeProcessor recipeProcessor in recipeProcessors) {
                if (processors.ContainsKey(recipeProcessor.id)) {
                    Debug.LogError("Duplicate id for recipe processors " + recipeProcessor.name + " and " + processors[recipeProcessor.id].name);
                    continue;
                }
                string path = AssetDatabase.GetAssetPath(recipeProcessor).Replace(recipeProcessor.name + ".asset", "");
                string[] directories = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
                Dictionary<int, Recipe[]> recipesOfMode = new Dictionary<int, Recipe[]>();
                foreach (string directorPath in directories) {
                    string[] split = directorPath.Split("/");
                    string directoryName = split[split.Length-1];
                    int index = -1;
                    try {
                        index = System.Convert.ToInt32(directoryName);
                    } catch (FormatException ex){
                        Debug.Log(ex);
                    }
                    if (index < 0) {
                        continue;
                    }
                    if (recipesOfMode.ContainsKey(index)) {
                        Debug.LogError("Duplicate mode folders for " + recipeProcessor.name);
                        continue;
                    } 
                    Recipe[] recipes = Resources.LoadAll<Recipe>(directorPath.Replace("Assets/Resources/",""));
                    foreach (Recipe recipe in recipes) {
                        loadItemList(recipe.inputs,recipe.InputPaths);
                        loadItemList(recipe.outputs,recipe.OutputPaths);
                        EditorUtility.SetDirty(recipe);
                    }
                    recipesOfMode[index] = recipes;
                    recipeCount+=recipes.Length;
                } 
                processors[recipeProcessor.id] = recipeProcessor;
                
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

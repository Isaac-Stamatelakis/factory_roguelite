using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RecipeRegistry 
{
    private static Dictionary<string,List<Recipe>> processors;
    private static RecipeRegistry instance;
    private RecipeRegistry() {
        int recipeCount = 0;
        processors = new Dictionary<string, List<Recipe>>();
        RecipeProcessor[] recipeProcessors = Resources.LoadAll<RecipeProcessor>("");
        foreach (RecipeProcessor recipeProcessor in recipeProcessors) {
            if (!processors.ContainsKey(recipeProcessor.id)) {
                List<Recipe> recipes = new List<Recipe>();
                foreach (Recipe recipe in Resources.LoadAll<Recipe>("Recipes/" + recipeProcessor.name)) {
                    loadItemList(recipe.inputs,recipe.InputPaths);
                    loadItemList(recipe.outputs,recipe.OutputPaths);
                    EditorUtility.SetDirty(recipe);
                    recipes.Add(recipe);
                    recipeCount ++;
                }
                processors[recipeProcessor.id] = recipes;
            }
        }
        Debug.Log("Recipe registry loaded " + processors.Count + " recipe processors and " + recipeCount + " recipes");
    }

    private void loadItemList(List<ScriptableItemSlot> items, List<string> paths) {
        for (int i = 0; i < items.Count; i ++) {
            ScriptableItemSlot itemSlot = items[i];
            if (itemSlot.item == null) {
                if (i < paths.Count) {
                    itemSlot.item = AssetDatabase.LoadAssetAtPath<ItemObject>(paths[i]);
                } else {
                    Debug.LogError("Recipe Registry tried to load itemobject as it was null from path which did not exist");
                }
            } else {
                if (paths.Count <= i) {
                    paths.Add(AssetDatabase.GetAssetPath(itemSlot.item));
                } else {
                    paths[i] = AssetDatabase.GetAssetPath(itemSlot.item);
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
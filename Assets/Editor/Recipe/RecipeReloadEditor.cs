using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using RecipeModule;

public class RecipeReloadWindow : EditorWindow {
    [MenuItem("Tools/Recipe/Reload")]
    public static void ShowWindow()
    {
        RecipeReloadWindow window = (RecipeReloadWindow)EditorWindow.GetWindow(typeof(RecipeReloadWindow));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Reload"))
        {
            reload();
        }
    }

    void reload()
    {
        Dictionary<string, RecipeProcessor> processors = new Dictionary<string, RecipeProcessor>();
        int recipeCount = 0;
        RecipeProcessor[] recipeProcessors = Resources.LoadAll<RecipeProcessor>("");
        foreach (RecipeProcessor recipeProcessor in recipeProcessors) {
            if (processors.ContainsKey(recipeProcessor.id)) {
                Debug.LogError("Duplicate id for recipe processors " + recipeProcessor.name + " and " + processors[recipeProcessor.id].name);
                continue;
            }
            string path = AssetDatabase.GetAssetPath(recipeProcessor).Replace(recipeProcessor.name + ".asset", "");
            string[] directoryPaths = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.AllDirectories);
            Dictionary<string, string> nameToPath = new Dictionary<string, string>();
            foreach (string directorPath in directoryPaths) {
                string[] split = directorPath.Split("/");
                string directoryName = split[split.Length-1];
                nameToPath[directoryName] = directorPath;
            }
            // Reset collection folder
            if (nameToPath.ContainsKey("Collections")) {
                Directory.Delete(nameToPath["Collections"]);
                AssetDatabase.CreateFolder(path,"Collections");
            }
            Dictionary<int, Recipe[]> recipesOfMode = new Dictionary<int, Recipe[]>();
            foreach (string directorPath in directoryPaths) {
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
                
                Recipe[] recipes = Resources.LoadAll<Recipe>(directorPath.Replace("Assets/Resources/",""));
                foreach (Recipe recipe in recipes) {
                    loadItemList(recipe.inputs,recipe.InputPaths);
                    loadItemList(recipe.outputs,recipe.OutputPaths);
                    EditorUtility.SetDirty(recipe);
                }
                RecipeCollection recipeCollection = ScriptableObject.CreateInstance<RecipeCollection>();
                recipeCollection.name = recipeProcessor.name + "_M" + index.ToString();
                recipeCollection.recipes = recipes;
                AssetDatabase.CreateAsset(recipeCollection,path+"/Collections/"+recipeCollection.name+".asset");
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
}

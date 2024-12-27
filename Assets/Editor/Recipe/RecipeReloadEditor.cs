using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using RecipeModule;
using Items;

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
        /*
        HashSet<RecipeProcessor> processors = new HashSet<RecipeProcessor>();
        int recipeCount = 0;
        RecipeProcessor[] recipeProcessors = Resources.LoadAll<RecipeProcessor>("");
        foreach (RecipeProcessor recipeProcessor in recipeProcessors) {
            if (recipeProcessor is not ITypedRecipeProcessor typedRecipeProcessor) {
                processors.Add(recipeProcessor);
                continue;
            }
            Type collectionType = typedRecipeProcessor.getCollectionType();
            typedRecipeProcessor.resetRecipeCollection();
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
                Directory.Delete(nameToPath["Collections"],true);
                nameToPath.Remove("Collections");
            }
            string reducedPath = path.Remove(path.Length-1);
            AssetDatabase.CreateFolder(reducedPath,"Collections");
            AssetDatabase.Refresh();
            Dictionary<int, Recipe[]> recipesOfMode = new Dictionary<int, Recipe[]>();
            foreach (KeyValuePair<string,string> kvp in nameToPath) {
                int index = -1;
                try {
                    index = System.Convert.ToInt32(kvp.Key);
                } catch (FormatException ex){
                    Debug.Log(ex);
                }
                if (index < 0) {
                    continue;
                }
                
                Recipe[] recipes = Resources.LoadAll<Recipe>(kvp.Value.Replace("Assets/Resources/",""));
                foreach (Recipe recipe in recipes) {
                    loadItemList(recipe.inputs,recipe.InputPaths);
                    if (recipe is MultiOutputRecipe multiOutputRecipe) {
                        loadItemList(multiOutputRecipe.outputs,multiOutputRecipe.OutputPaths);
                    } else if (recipe is SingleOutputRecipe singleOutputRecipe) {
                        loadItemList(new List<ItemSlot>{singleOutputRecipe.output},new List<string>{singleOutputRecipe.outputGUID});
                    }
                    
                    EditorUtility.SetDirty(recipe);
                }
                
                var recipeCollection = ScriptableObject.CreateInstance(collectionType);
                Array.Sort(recipes, (recipe1, recipe2) => recipe2.inputs.Count.CompareTo(recipe1.inputs.Count));
                if (recipeCollection is IRecipeCollection setable) {
                    setable.setRecipes(recipes);
                    setable.setMode(index);
                }
                recipeCollection.name = recipeProcessor.name + "_M" + index.ToString();
                typedRecipeProcessor.addRecipeCollection(recipeCollection,index);
                AssetDatabase.CreateAsset(recipeCollection,path+"Collections/"+recipeCollection.name+".asset");
                recipeCount+=recipes.Length;
            } 
            Debug.Log(recipeProcessor.name + " Loaded with "  + typedRecipeProcessor.getModeCount() + " modes and " + typedRecipeProcessor.getRecipeCount() + " recipes");
            processors.Add(recipeProcessor);
            
        }
        Debug.Log("******************************************************************");
        Debug.Log("Recipes reloaded\n" + processors.Count + " recipe processors and " + recipeCount + " recipes");
        */
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

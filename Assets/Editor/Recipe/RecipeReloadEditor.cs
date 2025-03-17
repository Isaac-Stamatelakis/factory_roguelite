using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System;
using Item.Slot;
using RecipeModule;
using Items;
using Recipe.Objects.Generation;
using Object = System.Object;

public class RecipeReloadWindow : EditorWindow {
    [MenuItem("Tools/Recipe/Generators")]
    public static void ShowWindow()
    {
        RecipeReloadWindow window = (RecipeReloadWindow)EditorWindow.GetWindow(typeof(RecipeReloadWindow));
        window.titleContent = new GUIContent("Recipe Generators");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Finds all Recipe Generators and calls them",EditorStyles.boldLabel);
        if (GUILayout.Button("Generate All"))
        {
            RegenerateAll();
        }
    }

    private void RegenerateAll()
    {
        string filter = "t:RecipeGenerator";
        string[] guids = AssetDatabase.FindAssets(filter);
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RecipeGenerator recipeGenerator = AssetDatabase.LoadAssetAtPath<RecipeGenerator>(path);
            if (!recipeGenerator) continue;
            count++;
            RecipeGeneratorUtils.GenerateRecipes(recipeGenerator,false);
        }
        Debug.Log($"Generated Recipes for {count} generators");
    }

    
}

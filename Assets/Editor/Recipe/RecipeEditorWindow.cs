using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Packages.Rider.Editor.Util;
using Recipe;
using Recipe.Objects;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class RecipeEditorWindow : EditorWindow
{


    [MenuItem("Tools/Recipe")]


    public static void ShowWindow()
    {
        RecipeEditorWindow window = (RecipeEditorWindow)EditorWindow.GetWindow(typeof(RecipeEditorWindow));
        window.titleContent = new GUIContent("Recipe Editor");
    }

    void OnGUI()
    {
        GUI.enabled = false;
        GUILayout.TextArea("Deletes null or invalid recipes in every RecipeCollection");
        GUI.enabled = true;
        
        if (GUILayout.Button("Delete Null Recipes"))
        {
            DeleteNullRecipes();
        }
    }

    void DeleteNullRecipes()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(RecipeCollection));
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RecipeCollection collection = AssetDatabase.LoadAssetAtPath<RecipeCollection>(path);
            if (!collection || collection.Recipes == null) continue;
            for (int i = collection.Recipes.Count - 1; i >= 0; i--)
            {
                RecipeObject recipe = collection.Recipes[i];
                if (recipe) continue;
                collection.Recipes.RemoveAt(i);
                count++;
            }
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssetIfDirty(collection);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Deleted {count} Null/Invalid Recipes");
    }

}

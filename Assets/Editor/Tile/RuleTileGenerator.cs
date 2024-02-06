using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class RuleTileGenerator : EditorWindow {
    private Texture2D texture;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/RuleTile")]
    public static void ShowWindow()
    {
        RuleTileGenerator window = (RuleTileGenerator)EditorWindow.GetWindow(typeof(RuleTileGenerator));
        window.titleContent = new GUIContent("Rule Tile Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Texture to Convert", EditorStyles.boldLabel);
        GUILayout.Label("Ensure texture is formatted correctly");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Sprite", texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Tile Item"))
        {
            createRuleTile();
        }
    }

    void createRuleTile()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogError("Tile Generation for "+  tileName + "Abanadoned as Folder already exists at EditorCreations");
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        RuleTile ruleTile = EditorRuleTileFactory.from64x64Texture(texture,"Assets/EditorCreations/" + tileName, tileName);
        AssetDatabase.CreateAsset(ruleTile, path + "T~" +tileName + ".asset");

        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.name = tileName;
        tileItem.tile = ruleTile;
        tileItem.id = tileName;
        tileItem.id = tileItem.id.ToLower().Replace(" ","_");

        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
    }
}

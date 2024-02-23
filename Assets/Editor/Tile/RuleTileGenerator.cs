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
        texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true) as Texture2D;
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
        IdRuleTile ruleTile = EditorFactory.ruleTilefrom64x64Texture(texture,"Assets/EditorCreations/" + tileName, tileName);
        TileItemEditorFactory.generateTileItem(
            tileName: tileName,
            tile: ruleTile,
            createFolder: false
        );
    }
}

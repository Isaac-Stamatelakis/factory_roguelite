using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles.IdTiles;

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
        GUILayout.Label("ENSURE PROPER FORMAT:\n[]\n[UP]\n[LEFT]\n[DOWN]\n[RIGHT]\n[UP,RIGHT]\n[UP,LEFT]\n[DOWN,LEFT]\n[DOWN,RIGHT]\n[LEFT,RIGHT]\n[UP,DOWN]\n[UP,LEFT,DOWN]\n[LEFT,DOWN,RIGHT]\n[UP,RIGHT,DOWN]\n[LEFT,UP,RIGHT]\n[UP,LEFT,DOWN,RIGHT]", EditorStyles.boldLabel);
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
        RuleTile ruleTile = EditorFactory.ruleTilefrom64x64Texture(texture,"Assets/EditorCreations/" + tileName, tileName);
        ItemEditorFactory.SaveTileWithName(ruleTile,tileName);
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: ruleTile,
            tileType: TileType.Block,
            createFolder: false
        );
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

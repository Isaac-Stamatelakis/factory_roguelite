using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles;

public class BackgroundGeneratorWindow : EditorWindow {
    private Texture2D texture;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/Background")]
    public static void ShowWindow()
    {
        BackgroundGeneratorWindow window = (BackgroundGeneratorWindow)EditorWindow.GetWindow(typeof(BackgroundGeneratorWindow));
        window.titleContent = new GUIContent("Background Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Texture to Convert", EditorStyles.boldLabel);
        GUILayout.Label("Ensure texture is 24 pixels by 24 pixels");
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
        if (GUILayout.Button("Generate Background Item"))
        {
            createTileItem();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void createTileItem()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        BackgroundRuleTile tile = EditorFactory.backgroundRuleTileFrom24x24Texture(texture,"Assets/EditorCreations/" + tileName, tileName);
        ItemEditorFactory.SaveTileWithName(tile,tileName);
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: tile,
            tileType: TileType.Background,
            createFolder: false
        );
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

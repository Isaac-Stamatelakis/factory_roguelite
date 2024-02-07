using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

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

    void createTileItem()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogError("Tile Generation for "+  tileName + "Abanadoned as Folder already exists at EditorCreations");
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        RuleTile ruleTile = EditorFactory.backgroundRuleTileFrom24x24Texture(texture,"Assets/EditorCreations/" + tileName, tileName);
        AssetDatabase.CreateAsset(ruleTile, path + "T~" +tileName + ".asset");

        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.name = tileName;
        tileItem.tile = ruleTile;
        tileItem.id = tileName;
        tileItem.tileType = TileType.Background;
        tileItem.id = tileItem.id.ToLower().Replace(" ","_");
        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
        Debug.Log("Background Tile Created at Path: " + path);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles.IdTiles;

public class TileMultiGeneratorWindow : EditorWindow {
    private string collectionName;
    private Texture2D texture;
    [MenuItem("Tools/Item Constructors/Tile/Multi-Standard")]
    public static void ShowWindow()
    {
        TileMultiGeneratorWindow window = (TileMultiGeneratorWindow)EditorWindow.GetWindow(typeof(TileMultiGeneratorWindow));
        window.titleContent = new GUIContent("Multi Tile Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Texture to Convert", EditorStyles.boldLabel);
        GUILayout.Label("Ensure texture is multiple of 16 in width and height");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Sprite", texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        collectionName = EditorGUILayout.TextField(collectionName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Tile Items"))
        {
            createTileItems();
        }
    }

    void createTileItems()
    {
        string path = "Assets/EditorCreations/" + collectionName + "/";
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", collectionName);
        string collectionPath = "Assets/EditorCreations/" + collectionName;
        Sprite[] sprites = EditorFactory.SpritesFromTexture(texture,"Assets/EditorCreations/" + collectionName, collectionName,16,16);
        int index = 0;
        foreach (Sprite sprite in sprites) {
            Tile tile = ItemEditorFactory.StandardTileCreator(sprite,TileColliderType.Tile);
            string tileName = collectionName + index.ToString();
            string tilePath = collectionPath + "/" + tileName + "/";
#pragma warning disable CS0618 // Type or member is obsolete
            ItemEditorFactory.GeneratedTileItem(
                tileName: tileName,
                tile: tile,
                tileType: TileType.Block,
                savePath: tilePath
            );
#pragma warning restore CS0618 // Type or member is obsolete
            index += 1;
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles.IdTiles;

public class RandomTileGenerator : EditorWindow {
    private string tileName;
    private Texture2D texture;
    private int width = 1;
    private int height = 1;
    [MenuItem("Tools/Item Constructors/Tile/Random")]
    public static void ShowWindow()
    {
        RandomTileGenerator window = (RandomTileGenerator)EditorWindow.GetWindow(typeof(RandomTileGenerator));
        window.titleContent = new GUIContent("Random Tile Generator");
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
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        GUILayout.Label("Enter Dimensions in tiles");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Width:", GUILayout.Width(50));
        width = EditorGUILayout.IntField(width);
        EditorGUILayout.LabelField("Height:", GUILayout.Width(50));
        height = EditorGUILayout.IntField(height);
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
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        
        string path = "Assets/EditorCreations/" + tileName + "/";
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        string collectionPath = "Assets/EditorCreations/" + tileName;
        AssetDatabase.Refresh();
        Sprite[] sprites = EditorFactory.spritesFromTexture(texture,"Assets/EditorCreations/" + tileName, tileName,width*16,height*16);
        AssetDatabase.Refresh();
        RandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
        randomTile.m_Sprites = sprites;
        randomTile.sprite = sprites[0];
        ItemEditorFactory.setTileTransformOffset(sprites[0],randomTile);
        AssetDatabase.Refresh();
        AssetDatabase.CreateAsset(randomTile,path + "T~" + tileName + ".asset");
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: randomTile,
            tileType: TileType.Block,
            createFolder: false
        );
#pragma warning restore CS0618 // Type or member is obsolete
        AssetDatabase.Refresh();
    }
}

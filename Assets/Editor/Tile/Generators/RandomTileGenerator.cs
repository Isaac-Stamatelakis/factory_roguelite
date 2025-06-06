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
    private readonly EditorTileItemRebuilder itemRebuilder = new EditorTileItemRebuilder();
    private int tileSize = 1;
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
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("TileSize (World Space (16x16):", GUILayout.Width(100));
        tileSize = EditorGUILayout.IntField(tileSize);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Tile Items"))
        {
            createTileItems();
        }
        itemRebuilder.DisplayGUI(Rebuild, ref tileName);
        
        
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
        string collectionPath = Path.Combine("Assets/EditorCreations", tileName);
        AssetDatabase.Refresh();
        var randomTile =  BuildRandomTile(collectionPath);
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

    RandomTile BuildRandomTile(string path)
    {
        Sprite[] sprites = EditorFactory.SpritesFromTexture(texture,path, tileName,16,16);
        AssetDatabase.Refresh();
        RandomTile randomTile = ScriptableObject.CreateInstance<RandomTile>();
        randomTile.m_Sprites = sprites;
        randomTile.sprite = sprites[0];
        ItemEditorFactory.setTileTransformOffset(sprites[0],randomTile);
        AssetDatabase.Refresh();
        AssetDatabase.CreateAsset(randomTile,Path.Combine(path, "T~" + tileName + ".asset"));
        return randomTile;
    }
    
    private void Rebuild()
    {
        itemRebuilder.DeleteOldTileAssets(false);
        string itemPath = itemRebuilder.GetItemPath();
        string itemFolderPath = Path.GetDirectoryName(itemPath);
        TileItem tileItem = itemRebuilder.TileItem;
        TileBase tileBase = tileItem.tile;
        if (tileBase is RandomTile randomTile)
        {
            Sprite[] sprites = EditorFactory.SpritesFromTexture(texture,itemFolderPath, tileName,tileSize*16,tileSize*16);
            AssetDatabase.Refresh();
            randomTile.m_Sprites = sprites;
            randomTile.sprite = sprites[0];
            EditorUtility.SetDirty(randomTile);
            AssetDatabase.SaveAssetIfDirty(randomTile);
            AssetDatabase.Refresh();
        }
        else
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tileBase));
            AssetDatabase.Refresh();
            RandomTile newRandomTile = BuildRandomTile(itemFolderPath);
            itemRebuilder.ReplaceTile(newRandomTile);
        }
        Debug.Log($"Rebuilt {tileName} at {itemPath}");
    }
}

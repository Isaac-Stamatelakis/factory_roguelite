using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles;
using Tiles.CustomTiles.StateTiles.Instances.Platform;

public class PlatformTileGenerator : EditorWindow {
    private Sprite connectAll;
    private Sprite connectOne;
    private Sprite connectNone;
    private Sprite slope;
    private Sprite slopeDeco;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/Platform")]
    public static void ShowWindow()
    {
        PlatformTileGenerator window = (PlatformTileGenerator)EditorWindow.GetWindow(typeof(PlatformTileGenerator));
        window.titleContent = new GUIContent("Platform Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        CreateSpriteField(ref connectAll, "FlatConnectAll");
        CreateSpriteField(ref connectOne, "FlatConnectOne");
        CreateSpriteField(ref connectNone, "FlatConnectNone");
        CreateSpriteField(ref slope, "Slope");
        CreateSpriteField(ref slopeDeco, "SlopeDeco");
        
        if (GUILayout.Button("Generate Platform Tile"))
        {
            CreateTileItem();
        }

        void CreateSpriteField(ref Sprite sprite, string fieldName)
        {
            EditorGUILayout.BeginHorizontal();
            sprite = EditorGUILayout.ObjectField(fieldName, sprite, typeof(Sprite), true) as Sprite;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void CreateTileItem()
    {
        string path = Path.Combine(EditorUtils.EDITOR_SAVE_PATH, tileName);
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder(EditorUtils.EDITOR_SAVE_PATH, tileName);
        AssetDatabase.CreateFolder(path, "Tiles");
        AssetDatabase.Refresh();
        string tilePath = Path.Combine(path, "Tiles");
        PlatformStateTile platformStateTile = ScriptableObject.CreateInstance<PlatformStateTile>();
        platformStateTile.name = tileName + "Tile";
        platformStateTile.FlatConnectOne = CreateSubTile(connectOne);
        platformStateTile.FlatConnectNone = CreateSubTile(connectNone);
        platformStateTile.FlatConnectAll = CreateSubTile(connectAll);
        platformStateTile.Slope = CreateSubTile(slope);
        platformStateTile.SlopeDeco = CreateSubTile(slopeDeco);
        
        AssetDatabase.CreateAsset(platformStateTile, Path.Combine(path,platformStateTile.name + ".asset"));
        AssetDatabase.SaveAssets();
        return;
        
        TileBase CreateSubTile(Sprite sprite)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.name = tileName + sprite.name + "Tile";
            AssetDatabase.CreateAsset(tile,Path.Combine(tilePath,tile.name+".asset"));
            return tile;
        }
    }
}

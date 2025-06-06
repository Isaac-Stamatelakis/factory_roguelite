using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles.IdTiles;

public class Standard1TileGeneratorWindow : EditorWindow {
    private Sprite sprite;
    private TileType tileType;
    private TileColliderType colliderType;
    private string tileName;
    private string path;
    private readonly EditorTileItemRebuilder rebuilder = new EditorTileItemRebuilder();
    [MenuItem("Tools/Item Constructors/Tile/Standard")]
    public static void ShowWindow()
    {
        Standard1TileGeneratorWindow window = (Standard1TileGeneratorWindow)EditorWindow.GetWindow(typeof(Standard1TileGeneratorWindow));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Collider:", GUILayout.Width(70));
        colliderType = (TileColliderType)EditorGUILayout.EnumPopup(colliderType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type:", GUILayout.Width(70));
        tileType = (TileType)EditorGUILayout.EnumPopup(tileType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
        
        rebuilder.DisplayGUI(RebuildTile, ref tileName);
    }

    private void createTileItem()
    {
        Tile tile = ItemEditorFactory.StandardTileCreator(sprite,colliderType);
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(tileName,tile,tileType);
#pragma warning restore CS0618 // Type or member is obsolete
        ItemEditorFactory.SaveTileWithName(tile,tileName);
        
    }

    private void RebuildTile()
    {
        TileItem tileItem = rebuilder.TileItem;
        TileBase tileBase = tileItem.tile;
        if (tileBase is not Tile tile)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tileBase));
            AssetDatabase.Refresh();
            
            Tile newTile = ItemEditorFactory.StandardTileCreator(sprite,colliderType);;
            string itemPath = rebuilder.GetItemPath();
            string itemFolderPath = Path.GetDirectoryName(itemPath);
            ItemEditorFactory.SaveTileWithName(newTile,tileName,path:itemFolderPath + '/');
            tileItem.tile = newTile;
            
            EditorUtility.SetDirty(tileItem);
            AssetDatabase.SaveAssetIfDirty(tileItem);
        }
        else
        {
            tile.sprite = sprite;
            ItemEditorFactory.SetTileTransformOffset(sprite,tile);
            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssetIfDirty(tile);
        }
    }
}

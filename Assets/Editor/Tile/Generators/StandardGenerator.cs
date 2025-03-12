using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class Standard1TileGeneratorWindow : EditorWindow {
    private Sprite sprite;
    private TileType tileType;
    private TileColliderType colliderType;
    private string tileName;
    private string path;
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
    }

    void createTileItem()
    {
        StandardTile tile = ItemEditorFactory.standardTileCreator(sprite,colliderType);
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(tileName,tile,tileType);
#pragma warning restore CS0618 // Type or member is obsolete
        ItemEditorFactory.saveTileWithName(tile,tileName);
        
    }
}

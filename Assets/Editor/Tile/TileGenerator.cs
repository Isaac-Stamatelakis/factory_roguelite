using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class TileGeneratorWindow : EditorWindow {
    private Sprite sprite;
    private string tileName;
    private string path;
    [MenuItem("Tools/Item Constructors/Tile/Standard/Tile")]
    public static void ShowWindow()
    {
        TileGeneratorWindow window = (TileGeneratorWindow)EditorWindow.GetWindow(typeof(TileGeneratorWindow));
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
        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
    }

    void createTileItem()
    {
        StandardTile tile = TileItemEditorFactory.standardTileCreator(sprite);
        TileItemEditorFactory.generateTileItem(tileName,tile,TileType.Block);
    }
}

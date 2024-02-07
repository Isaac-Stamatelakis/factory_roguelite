using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TileGeneratorWindow : EditorWindow {
    private Sprite sprite;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/Tile")]
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
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        StandardTile tile = ScriptableObject.CreateInstance<StandardTile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        Vector2Int spriteSize = Global.getSpriteSize(sprite);
        Matrix4x4 tileTransform = tile.transform;
        if (spriteSize.x % 2 == 0) {
            tileTransform.m03 = 0.25f;
        }
        if (spriteSize.y % 2 == 0) {
            tileTransform.m13 = 0.25f;
        }
        tile.transform = tileTransform;
        
        tile.name = "T~" + tileName;
        tileItem.name = tileName;
        tileItem.tile = tile;

        string path = "Assets/EditorCreations/" + tileName + "/";
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogError("Tile Generation for "+  tileItem + "Abanadoned as Folder already exists at EditorCreations");
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        
        tileItem.id = tileName;
        tileItem.id.ToLower().Replace(" ","_");
        
        
        AssetDatabase.CreateAsset(tile, path + tile.name + ".asset");
        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
        Debug.Log("TileItem and Tile Created for " + tileName + " at " + path);
    }
}

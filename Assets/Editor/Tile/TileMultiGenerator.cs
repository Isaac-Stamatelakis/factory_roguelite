using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class TileMultiGeneratorWindow : EditorWindow {
    private string collectionName;
    private Texture2D texture;
    [MenuItem("Tools/Item Constructors/Tile/Multi-Tile")]
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
            Debug.LogError("Tile Generation for "+  collectionName + "Abanadoned as Folder already exists at EditorCreations");
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", collectionName);
        string collectionPath = "Assets/EditorCreations/" + collectionName;
        Sprite[] sprites = EditorFactory.spritesFrom64x64Texture(texture,"Assets/EditorCreations/" + collectionName, collectionName);
        int index = 0;
        foreach (Sprite sprite in sprites) {
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
            
            tile.name = "T~" + collectionName + index.ToString();
            tileItem.name = collectionName + index.ToString();
            tileItem.tile = tile;

            string tilePath = collectionPath + "/" + tileItem.name + "/";
            
            AssetDatabase.CreateFolder(collectionPath, tileItem.name);
            tileItem.id = tileItem.name;
            tileItem.id.ToLower().Replace(" ","_");

            tile.id = tileItem.id;
            AssetDatabase.CreateAsset(tile, tilePath + tile.name + ".asset");
            AssetDatabase.CreateAsset(tileItem, tilePath + tileItem.name + ".asset");
            Debug.Log("TileItem and Tile Created for " + tileItem.name + " at " + tilePath);
            index += 1;
        }
        
    }
}

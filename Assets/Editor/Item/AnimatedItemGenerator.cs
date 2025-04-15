using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System.Linq;
using Items;

public class AnimatedItemGenerator : EditorWindow {
    private Sprite sprite;
    private TileType tileType;
    private TileColliderType colliderType;
    private string itemName;
    private string path;
    [MenuItem("Tools/Sprite/Animated")]
    public static void ShowWindow()
    {
        AnimatedItemGenerator window = (AnimatedItemGenerator)EditorWindow.GetWindow(typeof(AnimatedItemGenerator));
        window.titleContent = new GUIContent("Animated Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Item Name:", GUILayout.Width(70));
        itemName = EditorGUILayout.TextField(itemName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        
        if (GUILayout.Button("Generate"))
        {
            GenerateItem();
        }
    }

    private void GenerateItem()
    {
        const string ERROR_PREFIX = "Could not generated animated item: ";
        if (!sprite)
        {
            Debug.LogError($"{ERROR_PREFIX}Sprite is null");
            return;
        }

        string savePath = Path.Combine(EditorHelper.EDITOR_SAVE_PATH, itemName);
        if (AssetDatabase.IsValidFolder(savePath)) {
            Debug.LogWarning("Replaced existing content at " + savePath);
            Directory.Delete(savePath,true);
        }
        AssetDatabase.CreateFolder(EditorHelper.EDITOR_SAVE_PATH, itemName);
        Texture2D texture = sprite.texture;
        string assetPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (!textureImporter)
        {
            Debug.LogError($"{ERROR_PREFIX}Texture importer is null");
            return;
        }
        Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .ToArray();
        Sprite[] sprites = allSprites;
        SpriteCollection spriteCollection = ScriptableObject.CreateInstance<SpriteCollection>();
        spriteCollection.name = itemName + "Sprites";
        spriteCollection.Sprites = sprites;
        AssetDatabase.CreateAsset(spriteCollection,Path.Combine(savePath,spriteCollection.name + ".asset"));
        Debug.Log($"Created new animated item '{spriteCollection.name}' at path {AssetDatabase.GetAssetPath(spriteCollection)}");
    }
}

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
    [MenuItem("Tools/Item Constructors/Animated")]
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
        
        AnimatedCraftingItem animatedCraftingItem = ScriptableObject.CreateInstance<AnimatedCraftingItem>();
        animatedCraftingItem.name = itemName;
        animatedCraftingItem.Sprites = sprites;
        animatedCraftingItem.id = itemName.ToLower().Replace(" ", "_");
        AssetDatabase.CreateAsset(animatedCraftingItem,Path.Combine(EditorHelper.EDITOR_SAVE_PATH,animatedCraftingItem.name + ".asset"));
        Debug.Log($"Created new animated item '{animatedCraftingItem.name}' at path {AssetDatabase.GetAssetPath(animatedCraftingItem)}");
    }
}

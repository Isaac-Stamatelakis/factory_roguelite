using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class SpriteRuleSetter : EditorWindow {
    public Sprite sprite;
    public bool readWrite = true;
    public bool spriteSheet = false;

    [MenuItem("Tools/Sprite")]
    public static void ShowWindow()
    {
        SpriteRuleSetter window = (SpriteRuleSetter)EditorWindow.GetWindow(typeof(SpriteRuleSetter));
        window.titleContent = new GUIContent("Sprite Setter");
    }

    void OnGUI()
    {
        GUILayout.Label("When given sprite sets the rules to be correct", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;
        readWrite = EditorGUILayout.Toggle("Read/Write", readWrite);
        spriteSheet = EditorGUILayout.Toggle("SpriteSheet", spriteSheet);
        if (GUILayout.Button("Set"))
        {
            set();
        }
        EditorGUILayout.Space();
        
    }

    void set() {
        string path = AssetDatabase.GetAssetPath(sprite);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = readWrite;
        importer.spritePixelsPerUnit = 32;
        importer.filterMode = FilterMode.Point; 
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        if (spriteSheet) {
            importer.spriteImportMode = SpriteImportMode.Multiple;
        } else {
            importer.spriteImportMode = SpriteImportMode.Single;
        }
        importer.SaveAndReimport();
        EditorUtility.SetDirty(importer);
    }

}



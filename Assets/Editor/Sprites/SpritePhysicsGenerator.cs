using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class SpritePhysicsEditor : EditorWindow {
    public Sprite sprite;
    [MenuItem("ToolCollection/Sprite/Physics")]
    public static void ShowWindow()
    {
        SpritePhysicsEditor window = (SpritePhysicsEditor)EditorWindow.GetWindow(typeof(SpritePhysicsEditor));
        window.titleContent = new GUIContent("Sprite Physics");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        
        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;
        string path = AssetDatabase.GetAssetPath(sprite);
        
        if (GUILayout.Button("Set"))
        {
            string newPath = Path.Combine(EditorHelper.EDITOR_SAVE_PATH, sprite.name + "_copy.png");
            AssetDatabase.CopyAsset(path, newPath);
            Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(newPath);
            Texture2D texture = newSprite.texture;
            Color[] pixels = texture.GetPixels();

            // Modify the pixels here (example: changing all pixels to red)
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.red;  // This changes every pixel to red, modify as needed
            }
            Texture2D modifiedTexture = new Texture2D(texture.width, texture.height);
            // Set the modified pixels back to the new texture
            modifiedTexture.SetPixels(pixels);
            modifiedTexture.Apply();  // Apply the changes to the texture

            // Save the modified texture back to the disk
            byte[] bytes = modifiedTexture.EncodeToPNG();  // Save as PNG
            System.IO.File.WriteAllBytes(newPath, bytes);

            // Refresh the Asset Database to reflect the change
            AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }
        EditorGUILayout.Space();
        
    }

    

}


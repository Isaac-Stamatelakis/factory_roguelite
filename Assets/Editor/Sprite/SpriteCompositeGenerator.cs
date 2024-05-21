using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class SpriteCompositeGenerator : EditorWindow {
    private string spriteName;
    private Texture2D baseSprite;
    private Texture2D overlaySprite;



    [MenuItem("Tools/Sprite/Composite")]
    public static void ShowWindow()
    {
        SpriteCompositeGenerator window = (SpriteCompositeGenerator)EditorWindow.GetWindow(typeof(SpriteCompositeGenerator));
        window.titleContent = new GUIContent("Composite Sprite Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("When given sprite sets the rules to be correct", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        spriteName = EditorGUILayout.TextField("Sprite Name", spriteName,GUILayout.MinWidth(100));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        baseSprite = EditorGUILayout.ObjectField("Base Layer", baseSprite, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        overlaySprite = EditorGUILayout.ObjectField("Overlay Layer", overlaySprite, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Generate"))
        {
            overlay();
        }
        EditorGUILayout.Space();
        
    }

    private void overlay() {
        if (baseSprite.width != overlaySprite.width || baseSprite.height != overlaySprite.height) {
            Debug.LogError("Sprite dimensions do not match");
            return;
        }
        Color[] pixels = baseSprite.GetPixels(0,0,baseSprite.width,baseSprite.height);
        Color[] overlayPixels = overlaySprite.GetPixels(0,0,overlaySprite.width,overlaySprite.height);
        for (int x = 0; x < baseSprite.width; x++) {
            for (int y = 0; y < baseSprite.height; y++) {
                int index = x+y*baseSprite.width;
                Color overlayColor = overlayPixels[index];
                if (overlayColor.r == 0 && overlayColor.b == 0 && overlayColor.g == 0) {
                    continue;
                }
                pixels[index] = overlayColor;
            }
        }
        string savePath = Path.Combine(EditorHelper.EDITOR_SAVE_PATH,spriteName);
        Sprite sprite = TileSpriteShapeFactory.pixelsToSprite(pixels,savePath,baseSprite.width,baseSprite.height);

    }

    

}



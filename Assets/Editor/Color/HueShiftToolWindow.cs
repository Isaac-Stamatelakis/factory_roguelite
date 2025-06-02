using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles.CustomTiles.IdTiles;

public class HueShiftToolWindow : EditorWindow {
    private Sprite sprite;
    private TileType tileType;
    private TileColliderType colliderType;
    private string tileName;
    private string path;
    private Color primaryColor;
    private float hueDelta = 0.05f;
    private float lightnessDelta = 0.1f;
    private int range = 2;
    [MenuItem("Tools/Misc/Hue Shifter")]
    public static void ShowWindow()
    {
        HueShiftToolWindow window = (HueShiftToolWindow)EditorWindow.GetWindow(typeof(HueShiftToolWindow));
        window.titleContent = new GUIContent("Hue Shifter");
        
    }

    void OnGUI()
    {
        GUILayout.Label("Base Color Settings", EditorStyles.boldLabel);
        primaryColor = EditorGUILayout.ColorField("Primary Color", primaryColor);
        hueDelta = EditorGUILayout.FloatField("DeltaHue", hueDelta);
        lightnessDelta = EditorGUILayout.FloatField("DeltaLightness", lightnessDelta);
        range =  EditorGUILayout.IntField("Range", range);
        
        Color.RGBToHSV(primaryColor, out var h, out var s, out var v);
        
        GUILayout.Space(10);
        GUILayout.Label("Generated Variations", EditorStyles.boldLabel);
        for (int i = -range; i <= range; i++)
        {
            float hue = (h + i * hueDelta) % 1;
            float value = Mathf.Max(0, v + i * lightnessDelta); // Ensure it doesn't go below 0
            Color color = Color.HSVToRGB(hue, s, value);
            string colorName;
            switch (i)
            {
                case < 0:
                    colorName = $"Dark{Mathf.Abs(i)}";
                    break;
                case 0:
                    colorName = "Identity";
                    break;
                case > 0:
                    colorName = $"Light{Mathf.Abs(i)}";
                    break;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(colorName);
                EditorGUILayout.ColorField(GUIContent.none, color, false, false, false, GUILayout.Width(60));
                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    EditorGUIUtility.systemCopyBuffer = ColorUtility.ToHtmlStringRGB(color);
                }
            }
        }
    }
}
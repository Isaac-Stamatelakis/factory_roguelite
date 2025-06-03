using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class UIMaterialPropertyFixerWindow : EditorWindow
{
    [MenuItem("Tools/Misc/UIMaterial Properties")]
    
    
    public static void ShowWindow()
    {
        UIMaterialPropertyFixerWindow window = (UIMaterialPropertyFixerWindow)EditorWindow.GetWindow(typeof(UIMaterialPropertyFixerWindow));
        window.titleContent = new GUIContent("UI Material Property Assigner");
    }
    private const string SPRITE_LIT_KEYWORD = "USE_SHAPE_LIGHT_TYPE_0";
    void OnGUI()
    {
        if (GUILayout.Button("Apply Properties"))
        {
            ApplyProperties();
        }
    }

    void ApplyProperties()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(Material));
        int count = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.StartsWith("Assets/Material/UI") && !assetPath.StartsWith("Assets/Material/Items"))
            {
                continue;
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (IsSpriteLitShader(material)) continue;
            var shader = material.shader;
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            if (!shaderPath.EndsWith(".shadergraph")) continue;
            string json = File.ReadAllText(shaderPath);
            Debug.Log(json);
            break;
            Debug.Log(shader.name);
        }

        bool IsSpriteLitShader(Material material)
        {
            if (!material.shader) return false;
            foreach (string keyword in material.shader.keywordSpace.keywordNames)
            {
                if (keyword == SPRITE_LIT_KEYWORD)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

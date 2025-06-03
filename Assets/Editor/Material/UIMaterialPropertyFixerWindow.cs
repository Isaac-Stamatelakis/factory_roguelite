using System.Collections;
using System.Collections.Generic;
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
        foreach (string guid in guids)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (material.)
        }
    }
}

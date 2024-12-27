using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using TileEntity;
using System;
using System.Reflection;

public class TileEntitySoftLoadableSetter : EditorWindow {
    private string folderPath = "Assets/ScriptableObjects/Items";
    [MenuItem("Tools/TileEntity/SoftLoad")]
    public static void ShowWindow()
    {
        TileEntitySoftLoadableSetter window = (TileEntitySoftLoadableSetter)EditorWindow.GetWindow(typeof(TileEntitySoftLoadableSetter));
        window.titleContent = new GUIContent("Tile Entity Conduit Generation");
    }

    void OnGUI()
    {
        GUILayout.TextArea("Sets all TileEntities at a given path to SoftLoadable if their instance is an ISoftLoadableTileEntity", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Folder Path:", GUILayout.Width(100));
        folderPath = EditorGUILayout.TextField(folderPath);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Set Soft Loadable"))
        {
            set();
            
        }
    }

    private void set() {
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        int notSoftLoadedCounter = 0;
        int softLoadedCounter = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var tileEntity = AssetDatabase.LoadAssetAtPath<TileEntityObject>(assetPath);
            if (tileEntity == null) {
                continue;
            }
            
            ITileEntityInstance instance = tileEntity.createInstance(Vector2Int.zero,null,null);
            bool softLoadable = instance is ISoftLoadableTileEntity;
            tileEntity.SoftLoadable = softLoadable;
            if (softLoadable) {
                softLoadedCounter ++;
            } else {
                notSoftLoadedCounter++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Set {softLoadedCounter} Tile Entities are soft loaded and {notSoftLoadedCounter} as not soft loaded");
    }
}

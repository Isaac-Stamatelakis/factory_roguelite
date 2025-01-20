using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using WorldModule.Caves;
using DevTools.Structures;
using Chunks;
using WorldModule;
using Chunks.Partitions;
using Chunks.IO;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets.GUI;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;

/*
public enum PresetStructure {
    None,
    Dim0
}
public class StructureGenerator : EditorWindow {
    private static string dim0path = "Assets/ScriptableObjects/Structures/Dim0/Dim0.asset";
    private static readonly HashSet<PresetStructure> openStructurePresets = new HashSet<PresetStructure>();
    private static string[] structureNames = new string[]{"Restart this Editor"};
    private int index;
    private bool enforceEnclosure = true;
    private bool updateExisting = false;
    private PresetStructure preset;
    private string assetPath;
    [MenuItem("ToolCollection/Caves/Structure")]
    public static void ShowWindow()
    {
        structureNames = StructureGeneratorHelper.GetAllStructureFolders();
        StructureGenerator window = (StructureGenerator)EditorWindow.GetWindow(typeof(StructureGenerator));
        window.titleContent = new GUIContent("Structure Generator");
    }

    private void OnEnable()
    {
        structureNames = StructureGeneratorHelper.GetAllStructureFolders();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Update Existing Structure", GUILayout.Width(200));
        updateExisting = EditorGUILayout.Toggle(updateExisting);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (updateExisting) {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preset", GUILayout.Width(100));
            preset = (PresetStructure)EditorGUILayout.EnumPopup(preset);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        if (!updateExisting) {
            
        }
        

        if (preset == PresetStructure.None) {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Structures", GUILayout.Width(100));
            index = EditorGUILayout.Popup(index, structureNames);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enforce Enclosure", GUILayout.Width(200));
            enforceEnclosure = EditorGUILayout.Toggle(enforceEnclosure);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        if (updateExisting && preset == PresetStructure.None) {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Path", GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            assetPath = EditorGUILayout.TextField(assetPath,GUILayout.Width(200));
        }
        
        string buttonText = updateExisting ? "Update" : "Generate";
        if (GUILayout.Button(buttonText))
        {
            string structureName = structureNames[index];
            switch (preset) {
                case PresetStructure.Dim0:
                    structureName = "Dim0";
                    break;
            }
            if (preset != PresetStructure.None) {
                enforceEnclosure = openStructurePresets.Contains(preset);
            }
            LoadStructure(structureName);
        }
    }

    
}
*/

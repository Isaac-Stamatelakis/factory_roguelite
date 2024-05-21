using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using WorldModule.Caves;

public class EmptyCaveGenerator : EditorWindow {
    private string caveName;
    private GenerationModelType generationModelType;
    [MenuItem("Tools/Caves/Empty")]
    public static void ShowWindow()
    {
        EmptyCaveGenerator window = (EmptyCaveGenerator)EditorWindow.GetWindow(typeof(EmptyCaveGenerator));
        window.titleContent = new GUIContent("New Cave Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Cave Name:", GUILayout.Width(120));
        caveName = EditorGUILayout.TextField(caveName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Generation Model:", GUILayout.Width(120));
        generationModelType = (GenerationModelType)EditorGUILayout.EnumPopup(generationModelType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Cave"))
        {
            createTileItem();
        }
    }

    void createTileItem()
    {
        string path = "Assets/EditorCreations/" + caveName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", caveName);

        GenerationModel generationModel = null;
        switch (generationModelType) {
            case GenerationModelType.Cellular:
                CellularGeneratedArea cellularGeneratedArea = ScriptableObject.CreateInstance<CellularGeneratedArea>();
                cellularGeneratedArea.xInterval = new Vector2Int(-10,10);
                cellularGeneratedArea.yInterval = new Vector2Int(-10,10);
                cellularGeneratedArea.cellRadius = 2;
                cellularGeneratedArea.cellNeighboorCount = 14;
                cellularGeneratedArea.fillPercent = 0.42f;
                cellularGeneratedArea.smoothIterations = 5;
                cellularGeneratedArea.randomType = RandomType.Standard;
                generationModel = cellularGeneratedArea;
                break;
            default:
                Debug.LogWarning("EmptyCaveGenerated switch statement did not cover case for :" + generationModelType);
                break;
        }
        if (generationModel != null) {
            generationModel.name = caveName + " Model";
            AssetDatabase.CreateAsset(generationModel,path + generationModel.name + ".asset");
            AssetDatabase.Refresh();
        }
        
        Cave newArea = ScriptableObject.CreateInstance<Cave>();
        newArea.name = caveName;
        newArea.generationModel = generationModel;
        AssetDatabase.CreateAsset(newArea,path + caveName + ".asset");
        AssetDatabase.Refresh();


    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using System.Threading.Tasks;
using Tiles.CustomTiles;
using UnityEngine.AddressableAssets;
using WorldModule.Caves;

public class CaveSimulationWindow : EditorWindow
{
    private int simulationCount = 10;
    private List<CaveObject> caves;
    [MenuItem("Tools/Cave/Simulation Runner")]
    public static void ShowWindow()
    {
        CaveSimulationWindow window = (CaveSimulationWindow)EditorWindow.GetWindow(typeof(CaveSimulationWindow));
        window.titleContent = new GUIContent("Cave Simulation Runner");
    }

    public void OnEnable()
    {
        caves = new List<CaveObject>();
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(CaveObject));
        foreach (string guid in guids)
        {
            CaveObject caveObject = AssetDatabase.LoadAssetAtPath<CaveObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (!caveObject) continue;
            caves.Add(caveObject);
        }
    }


    void OnGUI()
    {
        GUI.enabled = false;
        GUILayout.TextArea($"Found {caves.Count} Caves. Current simulation count {simulationCount}");
        GUI.enabled = true;
        if (GUILayout.Button("Run Simulations"))
        {
            Simulate();
        }
    }

    void Simulate()
    {
        List<GenerationModel> generationModels = new List<GenerationModel>();
        foreach (CaveObject caveObject in caves)
        {
            GenerationModel generationModel = caveObject.generationModel;
            if (!generationModel) continue;
        }
        Debug.Log($"loaded {generationModels.Count}");
    }
    
    private GenerationModel.CaveSimulationResults SimulateResults(int[][] grid, int width, int height, GenerationModel generationModel)
    {
        int emptyCount = 0;
        int decorationCount = 0;
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x][y] == 0)
                {
                    emptyCount++;
                    continue;
                }
                
                int emptyNeighbors = 0;
                for (int i = 0; i < 4; i++)
                {
                    int checkX = x + dx[i];
                    int checkY = y + dy[i];
                    if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                    {
                        if (grid[checkX][checkY] == 0)
                        {
                            emptyNeighbors++;
                        }
                    }
                }

                bool sloped = emptyNeighbors == 2;
                if (sloped || emptyNeighbors == 0) continue;
                decorationCount++;
            }
        }
        float emptyRatio = (float)emptyCount / (width * height);
        float decorationRatioEstimate = (float)decorationCount / (width * height);
        return new GenerationModel.CaveSimulationResults(emptyRatio, decorationRatioEstimate);
    }
}

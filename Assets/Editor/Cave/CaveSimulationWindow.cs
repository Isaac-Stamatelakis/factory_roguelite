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
    private List<GenerationModel> genModels;
    [MenuItem("Tools/Cave/Simulation Runner")]
    public static void ShowWindow()
    {
        CaveSimulationWindow window = (CaveSimulationWindow)EditorWindow.GetWindow(typeof(CaveSimulationWindow));
        window.titleContent = new GUIContent("Cave Simulation Runner");
    }

    public void OnEnable()
    {
        genModels = new List<GenerationModel>();
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(CaveObject));
        foreach (string guid in guids)
        {
            CaveObject caveObject = AssetDatabase.LoadAssetAtPath<CaveObject>(AssetDatabase.GUIDToAssetPath(guid));
            if (!caveObject?.generationModel) continue;
            genModels.Add(caveObject.generationModel);
        }
    }


    void OnGUI()
    {
        GUI.enabled = false;
        GUILayout.TextArea($"Found {genModels.Count} Generation Models. Current simulation count {simulationCount}, estimated completion time {genModels.Count * simulationCount * 0.4f}s.");
        GUI.enabled = true;
        if (GUILayout.Button("Run Simulations"))
        {
            Simulate();
        }
    }

    void Simulate()
    {
        foreach (GenerationModel generationModel in genModels)
        {
            if (!generationModel) continue;
            GenerationModel.CaveSimulationResults total = new GenerationModel.CaveSimulationResults(0,0);
            for (int i = 0; i < simulationCount; i++)
            {
                var simulationResult = SimulateResults(512, 512, generationModel);
                total.EstimatedDecorationSpawnLocationRatio += simulationResult.EstimatedDecorationSpawnLocationRatio;
                total.EstimatedResultFillRatio += simulationResult.EstimatedResultFillRatio;
            }

            total.EstimatedDecorationSpawnLocationRatio /= simulationCount;
            total.EstimatedResultFillRatio /= simulationCount;
            generationModel.SimulationResults = total;
            EditorUtility.SetDirty(generationModel);
            AssetDatabase.SaveAssetIfDirty(generationModel);
            Debug.Log($"Simulation of {generationModel.name} complete");
        }
        Debug.Log($"Simulation of '{genModels.Count}' models complete");
    }
    
    private GenerationModel.CaveSimulationResults SimulateResults(int width, int height, GenerationModel generationModel)
    {
        int[][] grid = generationModel.GenerateGridInstant(UnityEngine.Random.Range(int.MinValue, int.MaxValue),new Vector2Int(width,height));
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

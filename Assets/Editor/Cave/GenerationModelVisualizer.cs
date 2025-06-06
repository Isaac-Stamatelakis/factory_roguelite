using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WorldModule.Caves;

[CustomEditor(typeof(GenerationModel),editorForChildClasses: true)]
public class GenerationModelVisualizer : Editor
{
    private Texture2D gridTexture;
    public void OnEnable() {

    }
    public override void OnInspectorGUI() {
        GenerationModel model = (GenerationModel)target;

        base.OnInspectorGUI();
        
        elementsBeforeVisualization();
        
        EditorGUILayout.LabelField("Simulation Results", EditorStyles.boldLabel);
        
        EditorGUILayout.LabelField("Fill Ratio");
        EditorGUI.BeginDisabledGroup(true); // Make read-only
        EditorGUILayout.Slider(model.FillEstimate, 0f, 1f);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.LabelField("Decoration Spawn Ratio");
        EditorGUI.BeginDisabledGroup(true); // Make read-only
        EditorGUILayout.Slider(model.DecorationRatioEstimate, 0f, 1f);
        EditorGUI.EndDisabledGroup();
        
        if (GUILayout.Button("Visualize"))
        {
            gridTexture = VisualizeGrid(model);
        }
        if (gridTexture == null) {
            gridTexture = VisualizeGrid(model);
        }
        GUILayout.Label("Cave Visualization");
        GUILayout.Label(gridTexture);
    }
    private Texture2D VisualizeGrid(GenerationModel model)
    {
        int width = 512;
        int height = 512;
        int[][] grid = model.GenerateGridInstant(Random.Range(0,100000),new Vector2Int(width,height));
        gridTexture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = (grid[x][y] == 1) ? Color.white : Color.black;
                gridTexture.SetPixel(x, y, color);
            }
        }

        FillSimulateResults(grid, width, height, model);
        gridTexture.Apply();
        return gridTexture;
    }
    
    private void FillSimulateResults(int[][] grid, int width, int height, GenerationModel generationModel)
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
        generationModel.SimulationResults = new GenerationModel.CaveSimulationResults(emptyRatio, decorationRatioEstimate);
    }
    protected virtual void elementsBeforeVisualization() {

    }
}
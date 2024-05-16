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
        if (GUILayout.Button("Visualize"))
        {
            gridTexture = visualizeGrid(model);
        }
        if (gridTexture == null) {
            gridTexture = visualizeGrid(model);
        }
        GUILayout.Label("Cave Visualization");
        GUILayout.Label(gridTexture);
    }
    private Texture2D visualizeGrid(GenerationModel model)
    {
        int[,] grid = model.generateGrid(Random.Range(0,100000),new Vector2Int(512,512));
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        gridTexture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = (grid[x, y] == 1) ? Color.black : Color.white;
                gridTexture.SetPixel(x, y, color);
            }
        }

        gridTexture.Apply();
        return gridTexture;
    }
    protected virtual void elementsBeforeVisualization() {

    }
}

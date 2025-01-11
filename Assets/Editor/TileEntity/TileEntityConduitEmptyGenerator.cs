using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class TileEntityConduitEmptyGenerator : EditorWindow {
    private string tileName;
    [MenuItem("ToolCollection/TileEntity/Empty")]
    public static void ShowWindow()
    {
        TileEntityConduitEmptyGenerator window = (TileEntityConduitEmptyGenerator)EditorWindow.GetWindow(typeof(TileEntityConduitEmptyGenerator));
        window.titleContent = new GUIContent("Tile Entity Conduit Generation");
    }

    void OnGUI()
    {
        GUILayout.Label("Ensure Game", EditorStyles.boldLabel);
        tileName = EditorGUILayout.TextField(tileName);
        if (GUILayout.Button("Generate Tile Item"))
        {
            if (tileName.Length != 0) {
                create();
            }
            
        }
    }

    void create()
    {
    
        GameObject container = initTilemap(tileName,null);
        initTilemap("Item",container.transform);
        initTilemap("Fluid",container.transform);
        initTilemap("Energy",container.transform);
        initTilemap("Signal",container.transform);

        string path = "Assets/EditorCreations/";
        PrefabUtility.SaveAsPrefabAsset(container,path + container.name + ".prefab");
        Debug.Log(container.name + " empty tilemap saved at " + path);
    }

    private GameObject initTilemap(string tileMapName, Transform parent) {
        GameObject emptyTileMapContainer = new GameObject();
        emptyTileMapContainer.name = tileMapName;
        emptyTileMapContainer.AddComponent<Tilemap>();
        emptyTileMapContainer.AddComponent<TilemapRenderer>();
        Grid grid = emptyTileMapContainer.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1f);
        if (parent != null) {
            emptyTileMapContainer.transform.SetParent(parent);
        }
        return emptyTileMapContainer;
    }
}

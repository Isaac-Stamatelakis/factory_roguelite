using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class DimCreatorWindow : EditorWindow {
    private static string GeneratePath = "Assets/EditorCreations";
    private Vector2Int xVec;
    private Vector2Int yVec;
    private StandardTile tileToPlace;

    [MenuItem("Tools/Tilemap")]
    public static void ShowWindow()
    {
        DimCreatorWindow window = (DimCreatorWindow)EditorWindow.GetWindow(typeof(DimCreatorWindow));
        window.titleContent = new GUIContent("Tilemap Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generates tilemaps of given chunk size", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        xVec.x = EditorGUILayout.IntField("X Min", xVec.x);
        xVec.y = EditorGUILayout.IntField("X Max", xVec.y);
        yVec.x = EditorGUILayout.IntField("Y Min", yVec.x);
        yVec.y = EditorGUILayout.IntField("Y Max", yVec.y);
        tileToPlace = EditorGUILayout.ObjectField("Tile", tileToPlace, typeof(StandardTile), true) as StandardTile;
        if (GUILayout.Button("Generate"))
        {
            generateTileMap();
        }
        EditorGUILayout.Space();
        
    }

    void generateTileMap() {
        GameObject tileMapContainer = new GameObject();
        tileMapContainer.name = "Tilemap";
        GameObject baseTileMap = new GameObject();
        baseTileMap.transform.SetParent(tileMapContainer.transform);
        Tilemap tilemap = baseTileMap.AddComponent<Tilemap>();
        baseTileMap.name = "Base";
        baseTileMap.AddComponent<TilemapRenderer>();
        Grid grid = baseTileMap.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1);
        for (int x = xVec.x*Global.ChunkSize; x < xVec.y*Global.ChunkSize; x++) {
            for (int y = yVec.x * Global.ChunkSize; y < yVec.y*Global.ChunkSize; y++) {
                tilemap.SetTile(new Vector3Int(x,y,0),tileToPlace);
            }
        }
        GameObject backgroundTileMap = new GameObject();
        backgroundTileMap.name = "Background";
        backgroundTileMap.transform.SetParent(tileMapContainer.transform);
        Tilemap tilemap1 = backgroundTileMap.AddComponent<Tilemap>();
        backgroundTileMap.AddComponent<TilemapRenderer>();
        Grid grid1 = backgroundTileMap.AddComponent<Grid>();
        grid1.cellSize = new Vector3(0.5f,0.5f,1);
        PrefabUtility.SaveAsPrefabAsset(tileMapContainer, GeneratePath + "/" + tileMapContainer.name + ".prefab");
    }

}




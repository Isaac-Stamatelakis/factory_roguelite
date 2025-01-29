using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class ConduitDimCreatorWindow : EditorWindow {
    private static string GeneratePath = "Assets/EditorCreations";
    private Vector2Int xVec;
    private Vector2Int yVec;
    private TileBase tileToPlace;

    [MenuItem("Tools/Tilemap/Conduit")]
    public static void ShowWindow()
    {
        ConduitDimCreatorWindow window = (ConduitDimCreatorWindow)EditorWindow.GetWindow(typeof(ConduitDimCreatorWindow));
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
        tileToPlace = EditorGUILayout.ObjectField("Tile", tileToPlace, typeof(TileBase), true) as TileBase;
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
        for (int cx = xVec.x; cx <= xVec.y; cx++) {
            for (int cy = yVec.x; cy <= yVec.y; cy++) {
                for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++) {
                    for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++) {
                        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                            for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                                if (cx == 0 && cy == 0) { // Leave empty space
                                    if ((px == 2 || px == 3) && (py == 2 || py == 3)) {
                                        continue;
                                    }
                                }
                                tilemap.SetTile(new Vector3Int(cx*Global.CHUNK_SIZE + px * Global.CHUNK_PARTITION_SIZE + x,cy * Global.CHUNK_SIZE + py * Global.CHUNK_PARTITION_SIZE+ y,0),tileToPlace);
                            }
                        }
                    }
                }
            }
        }
        initTileMap("Background",tileMapContainer.transform);
        initTileMap("ItemConduit",tileMapContainer.transform);
        initTileMap("FluidConduit",tileMapContainer.transform);
        initTileMap("EnergyConduit",tileMapContainer.transform);
        initTileMap("SignalConduit",tileMapContainer.transform);
        initTileMap("MatrixConduit",tileMapContainer.transform);
        PrefabUtility.SaveAsPrefabAsset(tileMapContainer, GeneratePath + "/" + tileMapContainer.name + ".prefab");
    }

    private GameObject initTileMap(string mapName, Transform parent) {
        GameObject tileMapObject = new GameObject();
        tileMapObject.name = mapName;
        tileMapObject.transform.SetParent(parent);
        Tilemap tilemap = tileMapObject.AddComponent<Tilemap>();
        tileMapObject.AddComponent<TilemapRenderer>();
        Grid grid = tileMapObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(0.5f,0.5f,1);
        return tileMapObject;
    }

}




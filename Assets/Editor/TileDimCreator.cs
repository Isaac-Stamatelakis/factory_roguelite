using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;

public class TileDimCreatorWindow : EditorWindow {
    private static string GeneratePath = "Assets/EditorCreations";
    private Vector2Int xVec;
    private Vector2Int yVec;
    private StandardTile tileToPlace;

    [MenuItem("ToolCollection/Tilemap/Standard")]
    public static void ShowWindow()
    {
        TileDimCreatorWindow window = (TileDimCreatorWindow)EditorWindow.GetWindow(typeof(TileDimCreatorWindow));
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
        for (int cx = xVec.x; cx <= xVec.y; cx++) {
            for (int cy = yVec.y; cy <= yVec.y; cy++) {
                for (int px = 0; px < Global.PARTITIONS_PER_CHUNK; px++) {
                    for (int py = 0; py < Global.PARTITIONS_PER_CHUNK; py++) {
                        for (int x = 0; x < Global.CHUNK_PARTITION_SIZE; x++) {
                            for (int y = 0; y < Global.CHUNK_PARTITION_SIZE; y++) {
                                if (cx == 0 && cy == 0) { // Leave empty space
                                    if ((px == 2 || px == 3) && (py == 2 || py == 3)) {
                                        continue;
                                    }
                                }
                                tilemap.SetTile(new Vector3Int(cx*Global.CHUNK_SIZE + px * Global.PARTITIONS_PER_CHUNK + x,cy * Global.CHUNK_SIZE + py * Global.PARTITIONS_PER_CHUNK+ y,0),tileToPlace);
                            }
                        }
                    }
                }
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
        GameObject.Destroy(tileMapContainer);
    }

}




using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Conduits;
using Items;
using NUnit.Framework.Constraints;
using Tiles;
using UnityEditor.Graphs;
using Color = UnityEngine.Color;

public class ConduitTileGenerator : EditorWindow {
    private Texture2D inactiveTexture;
    private Texture2D activeTexture;
    private string conduitName;
    private ConduitType conduitType;

    private static readonly int SIZE = 16;
    [MenuItem("Tools/Item Constructors/Conduit")]
    public static void ShowWindow()
    {
        ConduitTileGenerator window = (ConduitTileGenerator)EditorWindow.GetWindow(typeof(ConduitTileGenerator));
        window.titleContent = new GUIContent("Conduit Item Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        inactiveTexture = EditorGUILayout.ObjectField("Inactive Texture", inactiveTexture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        activeTexture = EditorGUILayout.ObjectField("Active Texture", activeTexture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Conduit Name:", GUILayout.Width(100));
        conduitName = EditorGUILayout.TextField(conduitName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        conduitType = (ConduitType)EditorGUILayout.EnumPopup("Conduit Type", conduitType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate"))
        {
            GenerateConduitTile();
        }
    }

    void GenerateConduitTile()
    {
        string path = "Assets/EditorCreations/" + conduitName;
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Deleted existing folder at " + path);
            Directory.Delete(path,true);
        }
        
        AssetDatabase.CreateFolder("Assets/EditorCreations", conduitName);
        AssetDatabase.Refresh();

        ConduitStateTile conduitTile = CreateInstance<ConduitStateTile>();
        List<Tile> tiles = new List<Tile>();
        AssetDatabase.CreateFolder(path, "Inactive");
        AssetDatabase.Refresh();
        string inactivePath = Path.Combine(path, "Inactive");
        
        tiles.AddRange(GenerateSpritesFromTexture(inactiveTexture,inactivePath));
        if (activeTexture != null)
        {
            string activePath = Path.Combine(path, "Active");
            AssetDatabase.CreateFolder(path, "Active");
            tiles.AddRange(GenerateSpritesFromTexture(activeTexture, activePath));
        }

        conduitTile.Tiles = new Tile[tiles.Count];
        for (int i = 0; i < tiles.Count; i++)
        {
            conduitTile.Tiles[i] = tiles[i];
        }
        
        ConduitItem conduitItem = null;
        switch (conduitType) {
            case ConduitType.Item:
                ResourceConduitItem resourceConduitItem = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem.type = ResourceConduitType.Item;
                conduitItem = resourceConduitItem;
                break;
            case ConduitType.Fluid:
                ResourceConduitItem resourceConduitItem1 = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem1.type = ResourceConduitType.Fluid;
                conduitItem = resourceConduitItem1;
                break;
            case ConduitType.Energy:
                ResourceConduitItem resourceConduitItem2 = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem2.type = ResourceConduitType.Energy;
                conduitItem = resourceConduitItem2;
                break;
            case ConduitType.Signal:
                conduitItem = ScriptableObject.CreateInstance<SignalConduitItem>();
                break;
            case ConduitType.Matrix:
                conduitItem = ScriptableObject.CreateInstance<MatrixConduitItem>();
                break;
        };

        if (conduitItem is null) { // should never get here
            Debug.LogError("Conduit Item Null");
            return;
        }
        conduitItem.name = conduitName;
        conduitItem.Tile = conduitTile;
        conduitItem.id = conduitName;
        conduitItem.id = conduitItem.id.ToLower().Replace(" ","_");
        
        conduitTile.name = $"T~{conduitItem.name}";
        AssetDatabase.CreateAsset(conduitTile, Path.Combine(path,conduitTile.name + ".asset"));
        AssetDatabase.CreateAsset(conduitItem, Path.Combine(path,conduitItem.name + ".asset"));
    }

    private Tile[] GenerateSpritesFromTexture(Texture2D texture, string spritePath)
    {
        Color[] pixels = texture.GetPixels();
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        int width = 0;
        for (int x = 0; x < SIZE; x++)
        {
            Color pixel = pixels[x];
            if (pixel.a == 0)
            {
                continue;
            }
            
            if (width == 0)
            {
                min.x = x;
            }
            width++;
        }

        max.x = min.x + width;
        int height = 0;
        for (int y = 0; y < SIZE; y++)
        {
            Color pixel = pixels[y*SIZE];
            if (pixel.a == 0)
            {
                continue;
            }
            if (height == 0)
            {
                min.y = y;
            }
            height++;
        }

        max.y = min.y + height;
        Debug.Log($"Generating sprites for {conduitName} with min {min} and max {max}");
        Tile[] tiles = new Tile[16];
        for (int i = 0; i < 16; i++)
        {
            bool up = (i & (int)ConduitDirectionState.Up) == 0;
            Color[] slicedPixels = texture.GetPixels();
            if (!up)
            {
                SlicePixels(new Vector2Int(0,max.y), new Vector2Int(SIZE,SIZE), slicedPixels);
            }
            
            bool down = (i & (int)ConduitDirectionState.Down) == 0;
            if (!down)
            {
                SlicePixels(new Vector2Int(0,0), new Vector2Int(SIZE,min.y), slicedPixels);
            }
    
            bool left = (i & (int)ConduitDirectionState.Left) == 0;
            if (!left)
            {
                SlicePixels(new Vector2Int(0,0), new Vector2Int(min.x,SIZE), slicedPixels);
            }
            
            bool right = (i & (int)ConduitDirectionState.Right) == 0;
            if (!right)
            {
                SlicePixels(new Vector2Int(max.x,0), new Vector2Int(SIZE,SIZE), slicedPixels);
            }

            string spriteName = $"{conduitName.ToLower().Replace(" ","_")}{i}";
            string savePath = $"{spritePath}/{spriteName}";
            Sprite sprite = TileSpriteShapeFactory.pixelsToSprite(slicedPixels,savePath,SIZE,SIZE);
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.colliderType = Tile.ColliderType.Grid;
            tile.sprite = sprite;
            tile.name = spriteName;
            ItemEditorFactory.saveTile(tile,savePath);
            tiles[i] = tile;
        }
        return tiles;
    }

    private void SlicePixels(Vector2Int min, Vector2Int max, Color[] pixels)
    {
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                pixels[x + y * SIZE] = new Color(0, 0, 0, 0);
            }
        }
    }
}

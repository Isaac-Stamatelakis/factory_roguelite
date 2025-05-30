using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;
using Tiles;

public class TileOutlineGeneratorWindow : EditorWindow {
    public enum OutlineTileType {
        Standard,
        HammerTile,
        NatureTile
    }

    public enum Scale
    {
        Scale1x,
        Scale2x,
    }
    private Texture2D texture;
    private Texture2D slabs;
    private Texture2D slants;
    private Texture2D perfectSlab;
    private Texture2D perfectSlant;
    private string tileName;
    private Color outlineColor = Color.white;
    private bool spriteSheet = false;
    private OutlineTileType outlineType;
    private static HammerTileValues hammerTileValues;
    private Vector2Int sliceSize = new Vector2Int(16,16);
    private Scale scale;
    [MenuItem("Tools/Item Constructors/Tile/Outline/Generator")]
    public static void ShowWindow()
    {
        TileOutlineGeneratorWindow window = (TileOutlineGeneratorWindow)EditorWindow.GetWindow(typeof(TileOutlineGeneratorWindow));
        window.titleContent = new GUIContent("Outline Generator");

    }
    private void OnEnable()
    {
        hammerTileValues ??= new HammerTileValues();
    }
    void OnGUI()
    {
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Texture",texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Outline Type:", GUILayout.Width(100));
        outlineType = (OutlineTileType)EditorGUILayout.EnumPopup(outlineType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Size:", GUILayout.Width(100));
        scale = (Scale)EditorGUILayout.EnumPopup(scale);
        if (outlineType == OutlineTileType.Standard) {
            spriteSheet = EditorGUILayout.Toggle("SpriteSheet",spriteSheet);
            if (spriteSheet) {
                GUILayout.Label("Sprite Size", EditorStyles.boldLabel);
                sliceSize.x = EditorGUILayout.IntField("X", sliceSize.x);
                sliceSize.y = EditorGUILayout.IntField("Y", sliceSize.y);
            }
        }
        
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void GenerateStandard(string path) {
        Sprite sprite = GenerateFromTexture(texture,path,"", tileName,scale);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = tileName;
        tile.sprite = sprite;
        AssetDatabase.CreateAsset(tile,path + tile.name + ".asset");
    }

    private void GenerateHammer(string path)
    {
        List<Sprite> sprites = new List<Sprite>();
        sprites.Add(GenerateFromTexture(texture, path, "", tileName,scale));
        sprites.Add(GenerateFromTexture(hammerTileValues.Slab.GetDefaultReadSprite().texture,path,"slab_",tileName,scale));
        sprites.Add(GenerateFromTexture(hammerTileValues.Slant.GetDefaultReadSprite().texture,path,"slant_",tileName,scale));
        sprites.Add(GenerateFromTexture(hammerTileValues.Stairs.GetDefaultReadSprite().texture,path,"stair_",tileName,scale));
        
        Tile baseTile = ItemEditorFactory.createTile(sprites[0],tileName,path);
        Tile slabTile = ItemEditorFactory.createTile(sprites[1],$"slab_{tileName}",path);
        Tile slantTile = ItemEditorFactory.createTile(sprites[2],$"slant_{tileName}",path);
        Tile stairTile = ItemEditorFactory.createTile(sprites[3],$"stair_{tileName}",path);
        HammerTile hammerTile = outlineType == OutlineTileType.HammerTile ? ScriptableObject.CreateInstance<HammerTile>() : CreateInstance<NatureTile>();
        hammerTile.baseTile = baseTile;
        hammerTile.cleanSlab = slabTile;
        hammerTile.cleanSlant = slantTile;
        hammerTile.stairs = stairTile;
        hammerTile.name = tileName;
        ItemEditorFactory.saveTile(hammerTile,path);
        if (outlineType != OutlineTileType.NatureTile) return;
        
        /*
        Tile[] natureSlabTiles = new Tile[hammerTileValues.NatureSlabs.Length];
        for (int i = 0; i < hammerTileValues.NatureSlabs.Length; i++)
        {
            Sprite sprite = GenerateFromTexture(hammerTileValues.NatureSlabs[i].GetDefaultReadSprite().texture,path,$"nature_slab_{i}_",tileName,scale);
            natureSlabTiles[i] = (ItemEditorFactory.createTile(sprite,$"slab_{tileName}_{i}",path));
        }
        */
        
        Tile[] natureSlantTiles = new Tile[hammerTileValues.NatureSlants.Length];
        for (int i = 0; i < hammerTileValues.NatureSlants.Length; i++)
        {
            Sprite sprite = GenerateFromTexture(hammerTileValues.NatureSlants[i].GetDefaultReadSprite().texture,path,$"nature_slant_{i}_",tileName,scale);
            natureSlantTiles[i] = (ItemEditorFactory.createTile(sprite,$"slant_{tileName}_{i}",path));
        }

        NatureTile natureTile = (NatureTile)hammerTile;
        natureTile.natureSlabs = Array.Empty<Tile>();
        natureTile.natureSlants = natureSlantTiles;
        natureTile.name = $"nature_{tileName}";
    }

    void Generate()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
    
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        if (outlineType == OutlineTileType.Standard) {
            GenerateStandard(path);
        } else if (outlineType == OutlineTileType.HammerTile || outlineType == OutlineTileType.NatureTile) {
            GenerateHammer(path);
        }
        
    }

    private static Sprite GenerateFromTexture(Texture2D texture, string path, string prefix, string tileName, Scale scale) {
        Color[] pixelsArr = texture.GetPixels();
        Color[,] pixels = EditorFactory.pixels1DTo2D(pixelsArr,texture.width,texture.height);
        if (scale == Scale.Scale2x)
        {
            pixels = ScalePixelArray(pixels, 2);
        }
        SpriteOutlineGenerator tileOutlineGenerator = new SpriteOutlineGenerator(true,pixels,Color.white);
        Color[,] outlinePixels = tileOutlineGenerator.generate();
        return EditorFactory.saveSprite(outlinePixels, path, $"{prefix}_{tileName}");
    }

    private static Color[,]  ScalePixelArray(Color[,] pixels, int scaleFactor)
    {
        Color[,] scaledPixels = new  Color[2*pixels.GetLength(0), 2*pixels.GetLength(1)];
        for (int x = 0; x < pixels.GetLength(0); x++)
        {
            for (int y = 0; y < pixels.GetLength(1); y ++)
            {
                for (int xs = 0; xs < scaleFactor; xs++)
                {
                    for (int ys = 0; ys < scaleFactor; ys++)
                    {
                        int idx = scaleFactor * x + xs;
                        int idy = scaleFactor * y + ys;
                        scaledPixels[idx,idy] = pixels[x,y];
                    }
                }
            }
        }
        return scaledPixels;
    }
}

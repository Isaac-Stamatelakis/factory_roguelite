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
    private Texture2D texture;
    private Texture2D slabs;
    private Texture2D slants;
    private Texture2D perfectSlab;
    private Texture2D perfectSlant;
    private string tileName;
    private Color outlineColor = Color.white;
    private bool spriteSheet = false;
    private bool padding = true;
    private OutlineTileType outlineType;
    private static HammerTileValues hammerTileValues;
    private Vector2Int sliceSize = new Vector2Int(16,16);

    [MenuItem("Tools/Item Constructors/Tile/Outline")]
    public static void ShowWindow()
    {
        TileOutlineGeneratorWindow window = (TileOutlineGeneratorWindow)EditorWindow.GetWindow(typeof(TileOutlineGeneratorWindow));
        window.titleContent = new GUIContent("Outline Generator");

    }
    private async void OnEnable()
    {
        if (hammerTileValues == null) {
            hammerTileValues = new HammerTileValues();
            await hammerTileValues.load();
        }
        
    }
    void OnGUI()
    {
        if (hammerTileValues == null)
        {
            GUILayout.Label("Loading...");
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        hammerTileValues.Texture = EditorGUILayout.ObjectField("Texture", hammerTileValues.Texture, typeof(Texture2D), true) as Texture2D;
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

        if (outlineType == OutlineTileType.NatureTile) {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            hammerTileValues.NatureSlabs = EditorGUILayout.ObjectField("Slabs", hammerTileValues.NatureSlabs, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            hammerTileValues.NatureSlants = EditorGUILayout.ObjectField("Slants", hammerTileValues.NatureSlants, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
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
            generate();
        }
    }
    void generateStandard(string path) {
        List<Sprite> sprites = generateFromTexture(hammerTileValues.Texture,spriteSheet,path,"");
        if (sprites.Count == 1) {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = tileName;
            tile.sprite = sprites[0];
            AssetDatabase.CreateAsset(tile,path + tile.name + ".asset");
            return;
        } 
        Debug.LogError("Standard tile recieved spritesheet");
        

    }

    void generateHammer(string path) {
        List<Sprite> sprites = generateFromTexture(hammerTileValues.Texture,false,path,"");
        sprites.AddRange(generateFromTexture(hammerTileValues.Slab,false,path,"slab_"));
        sprites.AddRange(generateFromTexture(hammerTileValues.Slant,false,path,"slant_"));
        Tile baseTile = ItemEditorFactory.createTile(sprites[0],tileName,path);
        Tile slabTile = ItemEditorFactory.createTile(sprites[1],$"slab_{tileName}",path);
        Tile slantTile = ItemEditorFactory.createTile(sprites[2],$"slant_{tileName}",path);
        if (outlineType != OutlineTileType.NatureTile) {
            HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
            hammerTile.baseTile = baseTile;
            hammerTile.cleanSlab = slabTile;
            hammerTile.cleanSlant = slantTile;
            hammerTile.name = $"{tileName}_hammer";
            ItemEditorFactory.saveTile(hammerTile,path);
            return;
        }
        List<Sprite> natureSlabs = generateFromTexture(hammerTileValues.NatureSlabs,true,path,"nature_slab");
        Tile[] natureSlabTiles = new Tile[natureSlabs.Count];
        for (int i = 0; i < natureSlabs.Count; i++) {
            natureSlabTiles[i] = (ItemEditorFactory.createTile(natureSlabs[i],$"slab_{tileName}_{i}",path));
        }

        List<Sprite> natureSlants = generateFromTexture(hammerTileValues.NatureSlants,true,path,"nature_slant");
        Tile[] natureSlantTiles = new Tile[natureSlants.Count];
        for (int i = 0; i < natureSlants.Count; i++) {
            natureSlantTiles[i] = (ItemEditorFactory.createTile(natureSlants[i],$"slant_{tileName}_{i}",path));
        }

        NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
        natureTile.baseTile = baseTile;
        natureTile.cleanSlab = slabTile;
        natureTile.cleanSlant = slantTile;
        natureTile.natureSlabs = natureSlabTiles;
        natureTile.natureSlants = natureSlantTiles;
        natureTile.name = $"nature_{tileName}";
        ItemEditorFactory.saveTile(natureTile,path);
    }

    void generate()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
    
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        if (outlineType == OutlineTileType.Standard) {
            generateStandard(path);
        } else if (outlineType == OutlineTileType.HammerTile || outlineType == OutlineTileType.NatureTile) {
            generateHammer(path);
        }
        
    }

    private List<Sprite> generateFromTexture(Texture2D texture, bool isSpriteSheet, string path, string prefix) {
        Vector2Int iterations = Vector2Int.one;
        if (isSpriteSheet) {
            bool error = false;
            
            if (texture.width % sliceSize.x == 0) {
                iterations.x = texture.width/sliceSize.x;
            } else {
                Debug.LogWarning($"Texture '{texture.name}' width is not divisible by slice slice {sliceSize.x}");
                error = true;
            }

            if (texture.height % sliceSize.y == 0) {
                iterations.y = texture.height/sliceSize.y;
            } else {
                Debug.LogWarning($"Texture '{texture.name}' height is not divisible by slice slice {sliceSize.y}");
                error = true;
            }
            if (error) {
                return null;
            }
        } else {
            sliceSize = new Vector2Int(texture.width,texture.height);
        }
        List<Sprite> sprites = new List<Sprite>();
        Color[] pixelList = texture.GetPixels();
        for (int x = 0; x < iterations.x; x ++) {
            for (int y = 0; y < iterations.y; y ++) {
                Color[] pixelsArr = texture.GetPixels(sliceSize.x*x,sliceSize.y*y,sliceSize.x,sliceSize.y);
                Color[,] pixels = EditorFactory.pixels1DTo2D(pixelsArr,sliceSize.x,sliceSize.y);
                SpriteOutlineGenerator tileOutlineGenerator = new SpriteOutlineGenerator(padding,pixels,outlineColor);
                Color[,] outlinePixels = tileOutlineGenerator.generate();
                
                sprites.Add(EditorFactory.saveSprite(outlinePixels,path,$"{prefix}_{tileName}[{x},{y}]"));
            }
        }
        return sprites;
    }
}

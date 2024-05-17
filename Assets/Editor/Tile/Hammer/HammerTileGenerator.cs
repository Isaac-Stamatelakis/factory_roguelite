using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
public enum TileVariation {
    Slant,
    Slab
}

public class StandardHammerTileGenerator : EditorWindow {
    private static readonly string defaultSlabPath = "Assets/Sprites/Shapes/slab_shapes.png";
    private static readonly string defaultSlantPath = "Assets/Sprites/Shapes/slant_shapes.png";
    private static readonly string perfectSlabPath = "Assets/Sprites/Shapes/perfect_slab.png";
    private static readonly string perfectSlantPath = "Assets/Sprites/Shapes/perfect_slant.png";
    
    public enum HammerTileType {
        Standard,
        Nature
    }
    
    private HammerTileType hammerTileType;
    private Texture2D texture;
    private static Texture2D slabs;
    private static Texture2D slants;
    private static Texture2D perfectSlab;
    private static Texture2D perfectSlant;
    private string tileName;
    private bool stateRotation = true;
    [MenuItem("Tools/Item Constructors/Tile/Hammer")]
    public static async void ShowWindow()
    {
        StandardHammerTileGenerator window = (StandardHammerTileGenerator)EditorWindow.GetWindow(typeof(StandardHammerTileGenerator));
        window.titleContent = new GUIContent("Tile Generator");
        await getBaseShapes();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(100));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Hammer Type:", GUILayout.Width(100));
        hammerTileType = (HammerTileType)EditorGUILayout.EnumPopup(hammerTileType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Use different tiles for rotation:", GUILayout.Width(200));
        stateRotation = EditorGUILayout.Toggle(stateRotation);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        if (hammerTileType == HammerTileType.Nature) {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            slabs = EditorGUILayout.ObjectField("Slabs", slabs, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            slants = EditorGUILayout.ObjectField("Slants", slants, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
    }

    private async static Task getBaseShapes() {
        var loadTasks = new Dictionary<string, Task<Texture2D>>();
        if (slabs == null) {
            loadTasks["slabs"] = Addressables.LoadAssetAsync<Texture2D>(defaultSlabPath).Task;
        }
        if (slants == null) {
            loadTasks["slants"] = Addressables.LoadAssetAsync<Texture2D>(defaultSlantPath).Task;
        }
        if (perfectSlab == null) {
            loadTasks["perfectSlab"] = Addressables.LoadAssetAsync<Texture2D>(perfectSlabPath).Task;
        }
        if (perfectSlant == null) {
            loadTasks["perfectSlant"] = Addressables.LoadAssetAsync<Texture2D>(perfectSlantPath).Task;
        }
        await Task.WhenAll(loadTasks.Values);
        if (loadTasks.ContainsKey("slabs") && slabs == null) {
            slabs = await loadTasks["slabs"];
        }
        if (loadTasks.ContainsKey("slants") && slants == null) {
            slants = await loadTasks["slants"];
        }
        if (loadTasks.ContainsKey("perfectSlab") && perfectSlab == null) {
            perfectSlab = await loadTasks["perfectSlab"];
        }
        if (loadTasks.ContainsKey("perfectSlant") && perfectSlant == null) {
            perfectSlant = await loadTasks["perfectSlant"];
        }
    }

    async void createTileItem()
    {
        await getBaseShapes();
        string path = Path.Combine("Assets/EditorCreations",tileName);
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        AssetDatabase.Refresh();


        switch (hammerTileType) {
            case HammerTileType.Standard:
                formStandard(path);
                break;
            case HammerTileType.Nature:
                formNature(path);
                break;

        }
    }

    private void formStandard(string path) {
        Tile baseTile = generateBase(texture,path);
        Tile slab = generateVariation(texture,path,"slab",perfectSlab.GetPixels(0,0,16,16));
        Tile slanted = generateVariation(texture,path,"slant",perfectSlant.GetPixels(0,0,16,16));

        HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
        hammerTile.id = ItemEditorFactory.formatId(tileName);
        hammerTile.baseTile = baseTile;
        hammerTile.cleanSlab = slab;
        hammerTile.cleanSlant = slanted;
        string hammerTilePath = Path.Combine(path,"T~"+tileName +"_Hammer" + ".asset");
        AssetDatabase.CreateAsset(hammerTile,hammerTilePath);
        AssetDatabase.Refresh();
        ItemEditorFactory.generateTileItem(tileName,hammerTile,TileType.Block,createFolder:false);
        AssetDatabase.Refresh();
    }
    private void formNature(string path) {
        NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
        Tile baseTile = generateBase(texture,path);
        natureTile.baseTile = baseTile;

        Tile slab = generateVariation(texture,path,"slab",perfectSlab.GetPixels(0,0,16,16));
        natureTile.cleanSlab = slab;

        Tile slanted = generateVariation(texture,path,"slant",perfectSlant.GetPixels(0,0,16,16));
        natureTile.cleanSlant = slanted;

        Tile[] natureSlabs = generateVariations(texture,path,"nature_slabs",slabs);
        natureTile.natureSlabs = natureSlabs;
    
        Tile[] natureSlants = generateVariations(texture,path,"nature_slants",slants);
        natureTile.natureSlants = natureSlants;

        string hammerTilePath = Path.Combine(path,"T~"+tileName +"_Nature" + ".asset");
        AssetDatabase.CreateAsset(natureTile,hammerTilePath);
        AssetDatabase.Refresh();

        ItemEditorFactory.generateTileItem(tileName,natureTile,TileType.Block,createFolder:false);
        AssetDatabase.Refresh();
    }
    private Tile generateBase(Texture2D texture, string path) {
        int width = texture.width/16;
        int height = texture.height/16;
        if (width < 1 || height < 1) {
            throw new System.Exception("Texture was less than 16 pixels wide and 16 pixels tall");
        }
        Tile tile = null;
        if (width == 1 && height == 1) {
            Sprite sprite = Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(0.5f,0.5f),32,0,SpriteMeshType.FullRect);
            StandardTile standardTile = ItemEditorFactory.standardTileCreator(sprite,TileColliderType.Tile);
            ItemEditorFactory.generateTileItem(tileName,standardTile,TileType.Block);
            ItemEditorFactory.saveTile(standardTile,tileName,path);
            tile = standardTile;
        } else {
            Sprite[] sprites = EditorFactory.spritesFromTexture(texture,path,tileName,16,16);
            IDRandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
            randomTile.name = tileName;
            randomTile.setID(ItemEditorFactory.formatId(tileName));
            randomTile.sprite = sprites[0];
            randomTile.m_Sprites = sprites;
            ItemEditorFactory.saveTile(randomTile,tileName);
            AssetDatabase.Refresh();
            tile = randomTile;
        }
        return tile;
    }

    private Tile generateVariation(Texture2D texture, string path, string variation, Color[] shape) {
        string variationPath = Path.Combine(path,variation);
        AssetDatabase.CreateFolder(path, variation);
        AssetDatabase.Refresh();
        int rotations = stateRotation ? 4 : 1;
        Tile[] tiles = new Tile[rotations];
        for (int r = 0; r < rotations; r++) {
            string rotationName;
            if (stateRotation) {
                rotationName  = $"{variation}_R{r*90}";
                AssetDatabase.CreateFolder(variationPath, rotationName);
                AssetDatabase.Refresh();
            } else {
                rotationName = tileName;
            }
            Sprite[] sprites = TileSpriteShapeFactory.generateSprites(texture,variationPath,rotationName,shape,r);
            if (sprites.Length == 1) {
                tiles[r] = ItemEditorFactory.standardTileCreator(sprites[0],TileColliderType.Sprite);
                ItemEditorFactory.saveTile(tiles[r],rotationName,path:path);
            } else {
                RandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
                randomTile.sprite = sprites[0];
                randomTile.m_Sprites = sprites;
                string savePath = Path.Combine(path,variation);
                ItemEditorFactory.saveTile(randomTile,rotationName,path:savePath+"\\");
                AssetDatabase.Refresh();
                tiles[r] = randomTile;
            }
        }

        Tile tile = null;
        if (stateRotation) {
            StateRotatableTile stateRotatableTile = ScriptableObject.CreateInstance<StateRotatableTile>();
            stateRotatableTile.Tiles = tiles;
            stateRotatableTile.name = variation;
            tile = stateRotatableTile;
            ItemEditorFactory.saveTile(tile,variation,path:path+"\\");
        } else {
            tile = tiles[0];
        }
        AssetDatabase.Refresh();
        return tile;
    }

    private Tile[] generateVariations(Texture2D texture, string path, string variation, Texture2D shapes) {
        AssetDatabase.CreateFolder(path,variation);
        string variationPath = Path.Combine(path,variation);
        int width = shapes.width/16;
        int height = shapes.height/16;
        Tile[] tiles = new Tile[width*height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color[] shapePixels = shapes.GetPixels(16*x,16*y,16,16);
                tiles[x+y*width] = generateVariation(texture,variationPath,$"{variation}[{x},{y}]",shapePixels);
            }
        }
        return tiles;
    }
    
}

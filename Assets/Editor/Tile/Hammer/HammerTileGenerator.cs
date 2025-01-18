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
    
    
    public enum HammerTileType {
        Standard,
        Nature
    }
    private HammerTileType hammerTileType;
    private static HammerTileValues hammerTileValues;
    private TileBase natureOutline;
    private string tileName;
    private bool stateRotation = true;
    private bool show = false;
    [MenuItem("ToolCollection/Item Constructors/Tile/Hammer")]
    public static void ShowWindow()
    {
        StandardHammerTileGenerator window = (StandardHammerTileGenerator)EditorWindow.GetWindow(typeof(StandardHammerTileGenerator));
        window.titleContent = new GUIContent("Tile Generator");
    }

    private async void OnEnable()
    {
        if (hammerTileValues == null) {
            hammerTileValues = new HammerTileValues();
            await hammerTileValues.load();
        }
        if (natureOutline == null) {
            natureOutline = await Addressables.LoadAssetAsync<TileBase>("Assets/Objects/Tiles/Outline/nature_Outline.asset").Task;
        }
        show = true;
        
    }

    void OnGUI()
    {
        if (!show) {
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        hammerTileValues.Texture = EditorGUILayout.ObjectField("Texture", hammerTileValues.Texture, typeof(Texture2D), true) as Texture2D;
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
            hammerTileValues.NatureSlabs = EditorGUILayout.ObjectField("Slabs", hammerTileValues.NatureSlabs, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            hammerTileValues.NatureSlants = EditorGUILayout.ObjectField("Slants", hammerTileValues.NatureSlants, typeof(Texture2D), true) as Texture2D;
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUI.enabled = false;

            var none = EditorGUILayout.ObjectField("Outline", natureOutline, typeof(Tile), true) as Tile;

            GUI.enabled = true;
        }

        

        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
    }

    void createTileItem()
    {
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
        Tile baseTile = generateBase(hammerTileValues.Texture,path);
        Tile slab = generateVariation(hammerTileValues.Texture,path,"slab",hammerTileValues.Slab.GetPixels(0,0,16,16));
        Tile slanted = generateVariation(hammerTileValues.Texture,path,"slant",hammerTileValues.Slant.GetPixels(0,0,16,16));
        Tile stairs = generateVariation(hammerTileValues.Texture,path,"stair",hammerTileValues.Stairs.GetPixels(0,0,16,16));
        
        HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
        hammerTile.id = ItemEditorFactory.formatId(tileName);
        hammerTile.baseTile = baseTile;
        hammerTile.cleanSlab = slab;
        hammerTile.cleanSlant = slanted;
        hammerTile.stairs = stairs;
        
        string hammerTilePath = Path.Combine(path,"T~"+tileName +"_Hammer" + ".asset");
        AssetDatabase.CreateAsset(hammerTile,hammerTilePath);
        AssetDatabase.Refresh();
        ItemEditorFactory.generateTileItem(tileName,hammerTile,TileType.Block,createFolder:false);
        AssetDatabase.Refresh();
    }
    private void formNature(string path) {
        NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
        Tile baseTile = generateBase(hammerTileValues.Texture,path);
        natureTile.baseTile = baseTile;

        Tile slab = generateVariation(hammerTileValues.Texture,path,"slab",hammerTileValues.Slab.GetPixels(0,0,16,16));
        natureTile.cleanSlab = slab;

        Tile slanted = generateVariation(hammerTileValues.Texture,path,"slant",hammerTileValues.Slant.GetPixels(0,0,16,16));
        natureTile.cleanSlant = slanted;
        
        Tile stairs = generateVariation(hammerTileValues.Texture,path,"stair",hammerTileValues.Stairs.GetPixels(0,0,16,16));
        natureTile.stairs = stairs;
        
        Tile[] natureSlabs = generateVariations(hammerTileValues.Texture,path,"nature_slabs",hammerTileValues.NatureSlabs);
        natureTile.natureSlabs = natureSlabs;
    
        Tile[] natureSlants = generateVariations(hammerTileValues.Texture,path,"nature_slants",hammerTileValues.NatureSlants);
        natureTile.natureSlants = natureSlants;

        string hammerTilePath = Path.Combine(path,"T~"+tileName +"_Nature" + ".asset");
        AssetDatabase.CreateAsset(natureTile,hammerTilePath);
        AssetDatabase.Refresh();

        ItemEditorFactory.generateTileItem(tileName,natureTile,TileType.Block,createFolder:false,outline:natureOutline);
        AssetDatabase.Refresh();
    }
    private Tile generateBase(Texture2D texture, string path) {
        int width = texture.width/16;
        int height = texture.height/16;
        if (width < 1 || height < 1) {
            throw new System.Exception("Texture was less than 16 pixels wide and 16 pixels tall");
        }
        Tile tile = null;
        Sprite[] sprites = EditorFactory.spritesFromTexture(texture,path,tileName,16,16);
        if (sprites.Length == 1) {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = tileName;
            tile.sprite = sprites[0];
        } else {
            IDRandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
            randomTile.name = tileName;
            randomTile.setID(ItemEditorFactory.formatId(tileName));
            randomTile.sprite = sprites[0];
            randomTile.m_Sprites = sprites;
            tile = randomTile;
        }
        ItemEditorFactory.saveTileWithName(tile,tileName);
        AssetDatabase.Refresh();
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
                rotationName  = $"{tileName}_{variation}_R{r*90}";
                AssetDatabase.CreateFolder(variationPath, rotationName);
                AssetDatabase.Refresh();
            } else {
                rotationName = tileName;
            }
            Sprite[] sprites = TileSpriteShapeFactory.generateSprites(texture,variationPath,rotationName,shape,r);
            if (sprites.Length == 1) {
                tiles[r] = ItemEditorFactory.standardTileCreator(sprites[0],TileColliderType.Sprite);
                ItemEditorFactory.saveTileWithName(tiles[r],rotationName,path:variationPath);
            } else {
                RandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
                randomTile.sprite = sprites[0];
                randomTile.m_Sprites = sprites;
                string savePath = Path.Combine(path,variation);
                ItemEditorFactory.saveTileWithName(randomTile,rotationName,path:savePath+"\\");
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
            ItemEditorFactory.saveTileWithName(tile,variation,path:path+"\\");
        } else {
            tile = tiles[0];
        }
        AssetDatabase.Refresh();
        return tile;
    }

    private Tile[] generateVariations(Texture2D texture, string path, string variation, Texture2D shapes) {
        if (shapes == null) {
            return null;
        }
        AssetDatabase.CreateFolder(path,variation);
        string variationPath = Path.Combine(path,variation);
        int width = shapes.width/16;
        int height = shapes.height/16;
        Tile[] tiles = new Tile[width*height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Color[] shapePixels = shapes.GetPixels(16*x,16*y,16,16);
                tiles[y+x*height] = generateVariation(texture,variationPath,$"{variation}[{x},{y}]",shapePixels);
            }
        }
        return tiles;
    }
    
}

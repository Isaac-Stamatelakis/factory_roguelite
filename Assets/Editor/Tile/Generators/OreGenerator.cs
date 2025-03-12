using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Items.Transmutable;
using Tiles;

public class OreTileGeneratorWindow : EditorWindow
{
    private const string DEFAULT_TEMPLATE_PATH = "Assets/Sprites/TransmutableSprites/ore_template-Sheet.png";
    private const int PIXELS_PER_TILE = 16;
    private Sprite sprite;
    private Sprite template;
    private TransmutableItemMaterial material;
    private TransmutableItemObject dropItem;
    
    private string stoneName;
    private string path;
    [MenuItem("Tools/Item Constructors/Tile/Ore")]
    public static void ShowWindow()
    {
        OreTileGeneratorWindow window = (OreTileGeneratorWindow)EditorWindow.GetWindow(typeof(OreTileGeneratorWindow));
        window.titleContent = new GUIContent("Tile Generator");
    }

    public void OnEnable()
    {
        template = AssetDatabase.LoadAssetAtPath<Sprite>(DEFAULT_TEMPLATE_PATH);
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        template = EditorGUILayout.ObjectField("Template Texture", template, typeof(Sprite), false) as Sprite;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        sprite = EditorGUILayout.ObjectField("Stone Sprite", sprite, typeof(Sprite), false) as Sprite;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        material = EditorGUILayout.ObjectField("Material", material, typeof(TransmutableItemMaterial), false) as TransmutableItemMaterial;
        
        dropItem = EditorGUILayout.ObjectField("Drop Ore", dropItem, typeof(TransmutableItemObject), false) as TransmutableItemObject;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stone Name", GUILayout.Width(100));
        stoneName = EditorGUILayout.TextField(stoneName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        
        if (GUILayout.Button("Generate"))
        {
            GenerateOre();
        }
    }

    private void GenerateOre()
    {
        string oreName = $"{material.name} {stoneName} Ore";
        
        string creationPath = EditorHelper.CreateFolder(oreName);
        
        Texture2D oreTexture = template.texture;
        Texture2D rockTexture = sprite.texture;
        Sprite[] sprites = GenerateMixedSprites(creationPath, oreTexture, rockTexture);
        
        RandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
        randomTile.m_Sprites = sprites;
        randomTile.sprite = sprites[0];
        ItemEditorFactory.setTileTransformOffset(sprites[0],randomTile);
        AssetDatabase.Refresh();
        AssetDatabase.CreateAsset(randomTile,creationPath + "T~" + oreName + ".asset");
        OutlineValues outlineValues = new OutlineValues();
        TileItem tileItem = ItemEditorFactory.GenerateTileItem(oreName, creationPath, randomTile, TileType.Block, outlineValues.SquareOutline);
        DropOption dropOption = new DropOption
        {
            itemObject = dropItem,
            weight = 1,
            lowerAmount = 1,
            upperAmount = 1
        };
        tileItem.tileOptions.dropOptions = new List<DropOption>
        {
            dropOption
        };
        AssetDatabase.SaveAssetIfDirty(tileItem);
        AssetDatabase.Refresh();
    }

    private Sprite[] GenerateMixedSprites(string creationPath, Texture2D oreTexture, Texture2D rockTexture)
    {
        const string SPRITE_FOLDER = "Sprites";
        string spritePath = Path.Combine(creationPath, SPRITE_FOLDER);
        AssetDatabase.CreateFolder(creationPath, SPRITE_FOLDER);
        
        Debug.Assert(oreTexture.width % PIXELS_PER_TILE == 0);
        Debug.Assert(oreTexture.height % PIXELS_PER_TILE == 0);
        Debug.Assert(rockTexture.width % PIXELS_PER_TILE == 0);
        Debug.Assert(rockTexture.height % PIXELS_PER_TILE == 0);
        
        int spriteCount = oreTexture.width/PIXELS_PER_TILE * oreTexture.height/PIXELS_PER_TILE * rockTexture.width/PIXELS_PER_TILE * rockTexture.height/PIXELS_PER_TILE;
        Sprite[] sprites = new Sprite[spriteCount];
        int index = 0;
        for (int ox = 0; ox < oreTexture.width / PIXELS_PER_TILE; ox++)
        {
            for (int oy = 0; oy < oreTexture.height / PIXELS_PER_TILE; oy++)
            {
                Color[] orePixels = oreTexture.GetPixels(ox*PIXELS_PER_TILE,oy*PIXELS_PER_TILE,PIXELS_PER_TILE,PIXELS_PER_TILE);
                Sprite[] interlacedSprites = InterlaceOreSpriteWithRockTexture(ox,oy, spritePath, orePixels,rockTexture);
                foreach (Sprite interlacedSprite in interlacedSprites)
                {
                    sprites[index] = interlacedSprite;
                    index++;
                }
            }
        }

        return sprites;
    }

    private Sprite[] InterlaceOreSpriteWithRockTexture(int ox, int oy, string creationPath, Color[] orePixels, Texture2D rockTexture)
    {
        int rockWidth = rockTexture.width / PIXELS_PER_TILE;
        int rockHeight = rockTexture.height / PIXELS_PER_TILE;
        Sprite[] sprites = new Sprite[rockWidth*rockHeight];
        for (int rx = 0; rx < rockWidth; rx++)
        {
            for (int ry = 0; ry < rockHeight; ry++)
            {
                Color[] rockPixels = rockTexture.GetPixels(rx*PIXELS_PER_TILE,ry*PIXELS_PER_TILE,PIXELS_PER_TILE,PIXELS_PER_TILE);
                
                // Override rock pixels with ore pixels that are not black

                for (int i = 0; i < orePixels.Length; i++)
                {
                    Color orePixel = orePixels[i];
                    if (orePixel == Color.black) continue;
                    rockPixels[i] = orePixel * material.color;
                }
                string savePath = Path.Combine(creationPath, $"ORE[{ox},{oy}]_ROCK[{rx}_{ry}]");
                Sprite rockOreSprite = TileSpriteShapeFactory.pixelsToSprite(rockPixels,savePath,PIXELS_PER_TILE,PIXELS_PER_TILE);
                sprites[rx + ry * rockWidth] = rockOreSprite;
            }
        }

        return sprites;
    }
}

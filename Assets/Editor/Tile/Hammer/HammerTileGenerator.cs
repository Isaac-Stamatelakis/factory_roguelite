using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace HammerTileEditor
{
    internal enum MultiTileType
    {
        Random,
        Animated
    }

    public class StandardHammerTileGenerator : EditorWindow
    {


        internal enum HammerTileType
        {
            Standard,
            Nature
        }

        private HammerTileType hammerTileType;
        private static HammerTileValues hammerTileValues;
        private TileBase natureOutline;
        private string tileName;
        private bool show = false;
        private Texture2D texture;
        private MultiTileType multiType;

        [MenuItem("ToolCollection/Item Constructors/Tile/Hammer")]
        public static void ShowWindow()
        {
            StandardHammerTileGenerator window =
                (StandardHammerTileGenerator)EditorWindow.GetWindow(typeof(StandardHammerTileGenerator));
            window.titleContent = new GUIContent("Tile Generator");
        }

        private void OnEnable()
        {
            if (hammerTileValues == null)
            {
                hammerTileValues = new HammerTileValues();
            }

            if (natureOutline == null)
            {
                //natureOutline = await Addressables.LoadAssetAsync<TileBase>("Assets/Objects/Tiles/Outline/nature_Outline.asset").Task;
            }

            show = true;

        }

        void OnGUI()
        {
            if (!show)
            {
                return;
            }

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

            if (GUILayout.Button("Generate Tile Item"))
            {
                createTileItem();
            }
        }

        void createTileItem()
        {
            string path = Path.Combine("Assets/EditorCreations", tileName);

            if (AssetDatabase.IsValidFolder(path))
            {
                Debug.LogWarning("Replaced existing content at " + path);
                Directory.Delete(path, true);
            }

            AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
            AssetDatabase.Refresh();


            switch (hammerTileType)
            {
                case HammerTileType.Standard:
                    formStandard(path);
                    break;
                case HammerTileType.Nature:
                    formNature(path);
                    break;

            }
        }

        private void formStandard(string path)
        {
            HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
            hammerTile.id = ItemEditorFactory.formatId(tileName);
            hammerTile.baseTile = generateBase(texture, path);
            hammerTile.cleanSlab = GenerateStateTile(texture, multiType, hammerTileValues.Slab, path, tileName, "slab");
            hammerTile.cleanSlant = GenerateStateTile(texture, multiType, hammerTileValues.Slant,path, tileName, "slant");
            hammerTile.stairs = GenerateStateTile(texture, multiType, hammerTileValues.Stairs,path, tileName, "stair");

            string hammerTilePath = Path.Combine(path, "T~" + tileName + "_Hammer" + ".asset");
            AssetDatabase.CreateAsset(hammerTile, hammerTilePath);
            AssetDatabase.Refresh();
            TileItem tileItem = ItemEditorFactory.generateTileItem(tileName, hammerTile, TileType.Block, createFolder: false);
            tileItem.tileOptions.StaticOptions.rotatable = true;
            tileItem.tileOptions.StaticOptions.hasStates = true;
            
            AssetDatabase.Refresh();
        }

        private void formNature(string path)
        {
            /*
            NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
            Tile baseTile = generateBase(hammerTileValues.Texture, path);
            natureTile.baseTile = baseTile;

            Tile slab = generateVariation(hammerTileValues.Texture, path, "slab",
                hammerTileValues.Slab.GetPixels(0, 0, 16, 16));
            natureTile.cleanSlab = slab;

            Tile slanted = generateVariation(hammerTileValues.Texture, path, "slant",
                hammerTileValues.Slant.GetPixels(0, 0, 16, 16));
            natureTile.cleanSlant = slanted;

            Tile stairs = generateVariation(hammerTileValues.Texture, path, "stair",
                hammerTileValues.Stairs.GetPixels(0, 0, 16, 16));
            natureTile.stairs = stairs;

            Tile[] natureSlabs = generateVariations(hammerTileValues.Texture, path, "nature_slabs",
                hammerTileValues.NatureSlabs);
            natureTile.natureSlabs = natureSlabs;

            Tile[] natureSlants = generateVariations(hammerTileValues.Texture, path, "nature_slants",
                hammerTileValues.NatureSlants);
            natureTile.natureSlants = natureSlants;

            string hammerTilePath = Path.Combine(path, "T~" + tileName + "_Nature" + ".asset");
            AssetDatabase.CreateAsset(natureTile, hammerTilePath);
            AssetDatabase.Refresh();

            ItemEditorFactory.generateTileItem(tileName, natureTile, TileType.Block, createFolder: false,
                outline: natureOutline);
            AssetDatabase.Refresh();
            */
        }

        private Tile generateBase(Texture2D texture, string path)
        {
            int width = texture.width / 16;
            int height = texture.height / 16;
            if (width < 1 || height < 1)
            {
                throw new System.Exception("Texture was less than 16 pixels wide and 16 pixels tall");
            }

            Tile tile = null;
            Sprite[] sprites = EditorFactory.spritesFromTexture(texture, path, tileName, 16, 16);
            if (sprites.Length == 1)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.name = tileName;
                tile.sprite = sprites[0];
            }
            else
            {
                IDRandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
                randomTile.name = tileName;
                randomTile.setID(ItemEditorFactory.formatId(tileName));
                randomTile.sprite = sprites[0];
                randomTile.m_Sprites = sprites;
                tile = randomTile;
            }

            ItemEditorFactory.saveTileWithName(tile, tileName);
            AssetDatabase.Refresh();
            return tile;
        }

        private static StateRotatableTile GenerateStateTile(Texture2D texture, MultiTileType multiTileType, SpriteRotationCollection sprites,
            string path, string name, string variation)
        {
            string variationPath = Path.Combine(path, variation);
            AssetDatabase.CreateFolder(path,variation);
            var tiles = new TileBase[4];
            string variationName = name + "_" + variation;
            var rotationNames = new List<string>
            {
                "0R", "90R", "180", "270"
            };
            if (sprites.Sprites.Length > 4)
                throw new ArgumentOutOfRangeException($"SpriteRotationCollection: {sprites.name} does not have 4 sprites");
            
            for (var i = 0; i < rotationNames.Count; i++)
            {
                string rotationName = rotationNames[i];
                string rotationPath = Path.Combine(variationPath, rotationName);
                AssetDatabase.CreateFolder(variationPath,rotationName);
                tiles[i] = GenerateTile(texture, multiTileType, sprites.Sprites[i], rotationPath, variationName);
                tiles[i].name = variationName + "_" + rotationName;
                AssetDatabase.CreateAsset(tiles[i], Path.Combine(rotationPath, tiles[i].name + ".asset"));
            }
            
            StateRotatableTile stateTile = ScriptableObject.CreateInstance<StateRotatableTile>();
            stateTile.name = variationName;
            stateTile.Tiles = tiles;
            string savePath = Path.Combine(variationPath, variationName + ".asset");
            AssetDatabase.CreateAsset(stateTile,savePath);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<StateRotatableTile>(savePath);
        }

        private static TileBase GenerateTile(Texture2D texture, MultiTileType type, ReadAndCopySprite readAndCopySprite, string path,
            string name)
        {
            Sprite[] sprites = CopyTextureIntoSprite(texture, readAndCopySprite, path, name);
            return FromSprites(sprites, type);

        }

        public static Sprite[] CopyTextureIntoSprite(Texture2D texture, ReadAndCopySprite readAndCopySprite, string path, string name)
        {
            const int height = 16;
            const int width = 16;

            int rows = texture.height / height;
            int columns = texture.width / width;
            Texture2D copySpriteTexture = readAndCopySprite.ReadSprite.texture;
            Color[] shapePixels = copySpriteTexture.GetPixels();
            Sprite[] sprites = new Sprite[rows * columns];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    string spritePath = AssetDatabase.GetAssetPath(readAndCopySprite.CopySprite);
                    string spriteSavePath = Path.Combine(path, name + "_" + index.ToString() + ".png");

                    AssetDatabase.CopyAsset(spritePath, spriteSavePath);
                    
                    Color[] pixels = texture.GetPixels(width * x, height * y, width, height);
                    Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath);
                    CopyShape(shapePixels, pixels);

                    Texture2D modifiedTexture = new Texture2D(texture.width, texture.height);
                    modifiedTexture.SetPixels(pixels);
                    modifiedTexture.Apply();
                    
                    byte[] bytes = modifiedTexture.EncodeToPNG();
                    File.WriteAllBytes(spriteSavePath, bytes);
                    
                    
                    
                    sprites[index] = newSprite;
                    
                }
            }

            return sprites;
        }

        private static void CopyShape(Color[] shape, Color[] pixels)
        {
            for (int x = 0; x < 16; x++) {
                for (int y = 0; y < 16; y++)
                {
                    int index = y * 16 + x;
                    if (shape[index].a == 0) {
                        pixels[y*16+x] = new Color(0,0,0,0);
                    }
                }
            }
        }

        private static TileBase FromSprites(Sprite[] sprites, MultiTileType type)
        {
            if (sprites.Length == 1)
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = sprites[0];
                return tile;
            }

            switch (type)
            {
                case MultiTileType.Random:
                    RandomTile randomTile = ScriptableObject.CreateInstance<RandomTile>();
                    randomTile.sprite = sprites[0];
                    randomTile.m_Sprites = sprites;
                    return randomTile;
                case MultiTileType.Animated:
                    AnimatedTile animatedTile = ScriptableObject.CreateInstance<AnimatedTile>();
                    animatedTile.m_AnimatedSprites = sprites;
                    return animatedTile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        /*

        private Tile generateVariation(Texture2D texture, string path, string variation, Color[] shape)
        {
            string variationPath = Path.Combine(path, variation);
            AssetDatabase.CreateFolder(path, variation);
            AssetDatabase.Refresh();
            int rotations = stateRotation ? 4 : 1;
            Tile[] tiles = new Tile[rotations];
            for (int r = 0; r < rotations; r++)
            {
                string rotationName;
                if (stateRotation)
                {
                    rotationName = $"{tileName}_{variation}_R{r * 90}";
                    AssetDatabase.CreateFolder(variationPath, rotationName);
                    AssetDatabase.Refresh();
                }
                else
                {
                    rotationName = tileName;
                }

                Sprite[] sprites =
                    TileSpriteShapeFactory.generateSprites(texture, variationPath, rotationName, shape, r);
                if (sprites.Length == 1)
                {
                    tiles[r] = ItemEditorFactory.standardTileCreator(sprites[0], TileColliderType.Sprite);
                    ItemEditorFactory.saveTileWithName(tiles[r], rotationName, path: variationPath);
                }
                else
                {
                    RandomTile randomTile = ScriptableObject.CreateInstance<IDRandomTile>();
                    randomTile.sprite = sprites[0];
                    randomTile.m_Sprites = sprites;
                    string savePath = Path.Combine(path, variation);
                    ItemEditorFactory.saveTileWithName(randomTile, rotationName, path: savePath + "\\");
                    AssetDatabase.Refresh();
                    tiles[r] = randomTile;
                }
            }

            Tile tile = null;
            if (stateRotation)
            {
                StateRotatableTile stateRotatableTile = ScriptableObject.CreateInstance<StateRotatableTile>();
                stateRotatableTile.Tiles = tiles;
                stateRotatableTile.name = variation;
                tile = stateRotatableTile;
                ItemEditorFactory.saveTileWithName(tile, variation, path: path + "\\");
            }
            else
            {
                tile = tiles[0];
            }

            AssetDatabase.Refresh();
            return tile;
        }

        private Tile[] generateVariations(Texture2D texture, string path, string variation, Texture2D shapes)
        {
            if (shapes == null)
            {
                return null;
            }

            AssetDatabase.CreateFolder(path, variation);
            string variationPath = Path.Combine(path, variation);
            int width = shapes.width / 16;
            int height = shapes.height / 16;
            Tile[] tiles = new Tile[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color[] shapePixels = shapes.GetPixels(16 * x, 16 * y, 16, 16);
                    tiles[y + x * height] =
                        generateVariation(texture, variationPath, $"{variation}[{x},{y}]", shapePixels);
                }
            }

            return tiles;
        }
        */
    }
    

}
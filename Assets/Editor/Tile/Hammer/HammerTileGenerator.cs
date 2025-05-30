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
using Tiles.CustomTiles.IdTiles;

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

        [System.Serializable]
        private class TextureNamePair
        {
            public Texture2D Texture;
            public string TileName;
        }

        private HammerTileType hammerTileType;
        private static HammerTileValues hammerTileValues;
        private OutlineValues outlineValues;
        private TileBase natureOutline;
        private List<TextureNamePair> texturePairs = new List<TextureNamePair>();
        private Vector2 scrollPosition;
        private MultiTileType multiType;
        private bool multipleTextures;

        [MenuItem("Tools/Item Constructors/Tile/Hammer")]
        public static void ShowWindow()
        {
            StandardHammerTileGenerator window = (StandardHammerTileGenerator)EditorWindow.GetWindow(typeof(StandardHammerTileGenerator));
            window.titleContent = new GUIContent("Tile Generator");
        }

        private void OnEnable()
        {
            outlineValues ??= new OutlineValues();
            hammerTileValues ??= new HammerTileValues();
            texturePairs = new List<TextureNamePair>
            {
                new()
            };
        }

        void OnGUI()
        {
            if (!multipleTextures)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                texturePairs[0].Texture = EditorGUILayout.ObjectField("Texture", texturePairs[0].Texture, typeof(Texture2D), true) as Texture2D;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(100));
                texturePairs[0].TileName = EditorGUILayout.TextField(texturePairs[0].TileName);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Multiple Textures:", GUILayout.Width(100));
            multipleTextures = EditorGUILayout.Toggle("Enable Multiple Textures", multipleTextures);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hammer Type:", GUILayout.Width(100));
            hammerTileType = (HammerTileType)EditorGUILayout.EnumPopup(hammerTileType);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (multipleTextures || (!ReferenceEquals(texturePairs[0].Texture,null) && (texturePairs[0].Texture.width > 16 || texturePairs[0].Texture.height > 16)))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Multi Type:", GUILayout.Width(100));
                multiType = (MultiTileType)EditorGUILayout.EnumPopup(multiType);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }
            

            if (GUILayout.Button("Generate Tile Item"))
            {
                foreach (TextureNamePair texturePair in texturePairs)
                {
                    CreateTileItem(texturePair.Texture,texturePair.TileName);
                }
                
            }

            if (multipleTextures)
            {
                GUILayout.Label("Texture-Tile Name Pairs", EditorStyles.boldLabel);
        
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < texturePairs.Count; i++)
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    texturePairs[i].Texture = (Texture2D)EditorGUILayout.ObjectField(
                        "Texture", 
                        texturePairs[i].Texture, 
                        typeof(Texture2D), 
                        false);
                    texturePairs[i].TileName = EditorGUILayout.TextField(
                        "Tile Name", 
                        texturePairs[i].TileName);
                    if (GUILayout.Button("Remove"))
                    {
                        if (texturePairs.Count == 1) return;
                        texturePairs.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                }
        
                EditorGUILayout.EndScrollView();
                if (GUILayout.Button("Add New Pair"))
                {
                    texturePairs.Add(new TextureNamePair());
                }
            }
            
        }

        private void CreateTileItem(Texture2D texture, string tileName)
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
                    FormStandard(texture, tileName, path);
                    break;
                case HammerTileType.Nature:
                    FormNature(texture, tileName, path);
                    break;

            }
            AssetDatabase.Refresh();
        }

        private void FormStandard(Texture2D texture, string tileName,string path)
        {
            HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
            AssignStandardHammerTiles(texture, tileName, hammerTile, path);
            TileItem tileItem = CreateItem(texture, tileName, hammerTile, path);
            tileItem.outline = outlineValues.HammerOutline;
        }

        private TileItem CreateItem(Texture2D texture, string tileName, HammerTile hammerTile, string path)
        {
            string hammerTilePath = Path.Combine(path, "T~" + tileName + ".asset");
            AssetDatabase.CreateAsset(hammerTile, hammerTilePath);
            AssetDatabase.Refresh();
#pragma warning disable CS0618 // Type or member is obsolete
            TileItem tileItem = ItemEditorFactory.GeneratedTileItem(tileName, hammerTile, TileType.Block, createFolder: false);
#pragma warning restore CS0618 // Type or member is obsolete
            tileItem.tileOptions.rotatable = true;
            EditorUtility.SetDirty(tileItem);
            
            return tileItem;
        }

        private void AssignStandardHammerTiles(Texture2D texture, string tileName, HammerTile hammerTile, string path)
        {
            hammerTile.baseTile = GenerateBase(texture, tileName, path);
            hammerTile.cleanSlab = GenerateStateTile(texture, multiType, hammerTileValues.Slab, path, tileName, "slab");
            hammerTile.cleanSlant = GenerateStateTile(texture, multiType, hammerTileValues.Slant,path, tileName, "slant");
            hammerTile.stairs = GenerateStateTile(texture, multiType, hammerTileValues.Stairs,path, tileName, "stair");
        }

        private void FormNature(Texture2D texture, string tileName, string path)
        {
            NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
            AssignStandardHammerTiles(texture, tileName, natureTile, path);
            natureTile.natureSlabs = Array.Empty<Tile>(); // Disabled nature slabs FormCollection(path, "nature_slabs", hammerTileValues.NatureSlabs);
            natureTile.natureSlabs = FormCollection(texture, tileName, path, "nature_slants", hammerTileValues.NatureSlants);
            
            TileItem tileItem = CreateItem(texture, tileName, natureTile, path);
            tileItem.outline = outlineValues.NatureOutline;;
        }

        private Tile[] FormCollection(Texture2D texture, string tileName, string path, string prefix, SpriteRotationCollection[] spriteCollections)
        {
            Tile[] tiles = new Tile[hammerTileValues.NatureSlabs.Length];
            string collectionPath = Path.Combine(path, prefix);
            AssetDatabase.CreateFolder(path, prefix);
            for (int i = 0; i < spriteCollections.Length; i++)
            {
                tiles[i] = GenerateStateTile(texture, multiType, spriteCollections[i], collectionPath, tileName, $"{prefix}{i}");
            }

            return tiles;
        }

        private TileBase GenerateBase(Texture2D texture, string tileName, string path)
        {
            int width = texture.width / 16;
            int height = texture.height / 16;
            if (width < 1 || height < 1)
            {
                throw new System.Exception("Texture was less than 16 pixels wide and 16 pixels tall");
            }
            
            Sprite[] sprites = EditorFactory.spritesFromTexture(texture, path, tileName, 16, 16);
            TileBase tile = GetTileFromSprites(tileName, sprites);

            tile.name = "_base_" + tileName;
            AssetDatabase.CreateAsset(tile,Path.Combine(path,tile.name+".asset"));
            AssetDatabase.Refresh();
            return tile;
        }

        private TileBase GetTileFromSprites(string tileName, Sprite[] sprites)
        {
            // This function probably already exists somewhere else
            if (sprites.Length == 1)
            {
                Tile singleTile = ScriptableObject.CreateInstance<Tile>();
                singleTile.name = tileName;
                singleTile.sprite = sprites[0];
                singleTile.colliderType = Tile.ColliderType.Grid;
                return singleTile;
            }
            
            switch (multiType)
            {
                case MultiTileType.Random:
                    RandomTile randomTile = ScriptableObject.CreateInstance<RandomTile>();
                    randomTile.name = tileName;
                    randomTile.sprite = sprites[0];
                    randomTile.m_Sprites = sprites;
                    randomTile.colliderType = Tile.ColliderType.Grid;
                    return randomTile;
                case MultiTileType.Animated:
                    AnimatedTile animatedTile = ScriptableObject.CreateInstance<AnimatedTile>();
                    animatedTile.name = tileName;
                    animatedTile.m_AnimatedSprites = sprites;
                    animatedTile.m_TileColliderType = Tile.ColliderType.Grid;
                    return animatedTile;
                default:
                    throw new ArgumentOutOfRangeException();
            }
                
            
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
                tiles[i].name = "_" + variationName + "_" + rotationName;
                AssetDatabase.CreateAsset(tiles[i], Path.Combine(rotationPath, tiles[i].name + ".asset"));
            }
            
            StateRotatableTile stateTile = ScriptableObject.CreateInstance<StateRotatableTile>();
            stateTile.name = "_" + variationName;
            stateTile.Tiles = tiles;
            string savePath = Path.Combine(variationPath, stateTile.name + ".asset");
            AssetDatabase.CreateAsset(stateTile,savePath);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<StateRotatableTile>(savePath);
        }

        private static TileBase GenerateTile(Texture2D texture, MultiTileType type, Sprite shapeSprite, string path,
            string name)
        {
            Sprite[] sprites = CopyTextureIntoSprite(texture, shapeSprite, path, name);
            return FromSprites(sprites, type);

        }

        public static Sprite[] CopyTextureIntoSprite(Texture2D texture, Sprite shapeSprite, string path, string name)
        {
            const int height = 16;
            const int width = 16;

            int rows = texture.height / height;
            int columns = texture.width / width;
            Texture2D copySpriteTexture = shapeSprite.texture;
            Color[] shapePixels = copySpriteTexture.GetPixels();
            Sprite[] sprites = new Sprite[rows * columns];
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    int index = y * columns + x;
                    string spritePath = AssetDatabase.GetAssetPath(shapeSprite);
                    string spriteSavePath = Path.Combine(path, "_" + name + "_" + index.ToString() + ".png");

                    AssetDatabase.CopyAsset(spritePath, spriteSavePath);
                    
                    Color[] pixels = texture.GetPixels(width * x, height * y, width, height);
                    Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath);
                    CopyShape(shapePixels, pixels);

                    Texture2D modifiedTexture = new Texture2D(width, height);
                    modifiedTexture.SetPixels(pixels);
                    modifiedTexture.Apply();
                    
                    byte[] bytes = modifiedTexture.EncodeToPNG();
                    File.WriteAllBytes(spriteSavePath, bytes);
                    
                    TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spriteSavePath);
                    importer.isReadable = false;
                    importer.spritePixelsPerUnit = 32;
                    importer.filterMode = FilterMode.Point; 
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                    EditorUtility.SetDirty(importer); 
                    
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
                    randomTile.colliderType = Tile.ColliderType.Sprite;
                    return randomTile;
                case MultiTileType.Animated:
                    AnimatedTile animatedTile = ScriptableObject.CreateInstance<AnimatedTile>();
                    animatedTile.m_AnimatedSprites = sprites;
                    animatedTile.m_TileColliderType = Tile.ColliderType.Sprite;
                    return animatedTile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
    

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class EditorFactory
{
    /* 
        Mappings from Sprites to Rule Tile
        Y = {u,r,d,l}
        f : Sprites → P(Y)
        f(0) → []
        f(1) → [u]
        f(2) → [r]
        f(3) → [d]
        f(4) → [l]
        f(5) → [u,l]
        f(6) → [u,r]
        f(7) → [r,d]
        f(8) → [d,l]
        f(9) → [r,l]
        f(10) → [u,d]
        f(11) → [u,r,d]
        f(12) → [r,d,l]
        f(13) → [u,d,l]
        f(14) → [u,r,l]
        f(15) → [u,r,d,l]
    */
    public static IdRuleTile ruleTilefrom64x64Texture(Texture2D texture, string spritePath, string name) {
        Vector2 textureSize = new Vector2(texture.width,texture.height);
        Vector2Int adjustedTextureSize = new Vector2Int(Mathf.FloorToInt(textureSize.x/16f),Mathf.FloorToInt(textureSize.y/16f));
        if (adjustedTextureSize.x != 4 || adjustedTextureSize.y != 4) {
            Debug.Log("Invalid dimensions for creating ruletile " + adjustedTextureSize + ". Must be 4 x 4");
        }
        
        IdRuleTile ruleTile = ScriptableObject.CreateInstance<IdRuleTile>();
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();
        
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        Sprite[] sprites = new Sprite[16];
        spritePath += "/Sprites/";
        for (int y = 0; y < 4 ; y++) {
            for (int x = 0; x < 4; x ++) {
                int index = y*4+x;
                RuleVal ruleVal = indexToRule(index);

                Color[] pixels = texture.GetPixels(texture.width*x/4,48-texture.height*y/4,16,16);
                Texture2D spliteTexture = new Texture2D(16,16);
                spliteTexture.SetPixels(0,0/4,16,16,pixels);
                

                string spriteSavePath = spritePath + "S~" + name +"-" + ruleVal.ToString();
                byte[] pngBytes = spliteTexture.EncodeToPNG();
                File.WriteAllBytes(spriteSavePath+".png", pngBytes);
                AssetDatabase.Refresh();

                TextureImporter textureImporter = AssetImporter.GetAtPath(spriteSavePath + ".png") as TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spritePixelsPerUnit = 32;
                AssetDatabase.ImportAsset(spriteSavePath + ".png", ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath + ".png");
                SpriteEditorHelper.set(sprite1,false,false);
                AssetDatabase.Refresh();
                sprites[index]=sprite1;


                
                RuleTile.TilingRule rule = new RuleTile.TilingRule();
                rule.m_ColliderType = Tile.ColliderType.Grid;
                rule.m_Sprites = new Sprite[1];
                rule.m_Sprites[0] = sprite1;
                
                switch (ruleVal) {
                    case RuleVal.Empty:
                        rule.m_Neighbors = new List<int>() {
                            0,2,0,2,2,0,2,0
                        };
                        break;
                    case RuleVal.U:
                        rule.m_Neighbors = new List<int> {
                            0,1,0,2,2,0,2,0
                        };
                        break;
                    case RuleVal.R:
                        rule.m_Neighbors = new List<int> {
                           0,2,0,1,2,0,2,0
                        };
                        break;
                    case RuleVal.D:
                        rule.m_Neighbors = new List<int> {
                             0,2,0,2,2,0,1,0
                        };
                        break;
                    case RuleVal.L:
                        rule.m_Neighbors = new List<int> {
                            0,2,0,2,1,0,2,0
                        };
                        break;
                    case RuleVal.UL:
                        rule.m_Neighbors = new List<int> {
                            0,1,0,2,1,0,2,0
                        };
                        break;
                    case RuleVal.UR:
                        rule.m_Neighbors = new List<int> {
                            0,1,0,1,2,0,2,0
                        };
                        break;
                    case RuleVal.RD:
                        rule.m_Neighbors = new List<int>() {
                            0,2,0,1,2,0,1,0
                        };
                        break;
                    case RuleVal.DL:
                        rule.m_Neighbors = new List<int>() {
                            0,2,0,2,1,0,1,0
                        };
                        break;
                    case RuleVal.LR:
                        rule.m_Neighbors = new List<int>() {
                            0,2,0,1,1,0,2,0
                        };
                        break;
                    case RuleVal.UD:
                        rule.m_Neighbors = new List<int>() {
                            0,1,0,2,2,0,1,0
                        };
                        break;
                    case RuleVal.URD:
                        rule.m_Neighbors = new List<int>() {
                            0,1,0,1,2,0,1,0
                        };
                        break;
                    case RuleVal.RDL:
                       rule.m_Neighbors = new List<int>() {
                            0,2,0,1,1,0,1,0
                        };
                        break;
                    case RuleVal.UDL:
                        rule.m_Neighbors = new List<int>() {
                            0,1,0,2,1,0,1,0
                        };
                        break;
                    case RuleVal.URL:
                        rule.m_Neighbors = new List<int>() {
                            0,1,0,1,1,0,2,0
                        };
                        break;
                    case RuleVal.URDL:
                        rule.m_Neighbors = new List<int>() {
                            0,1,0,1,1,0,1,0
                        };
                        break;
                    default:
                        Debug.LogError("Invalid rule for ruletile");
                        break;
                }
                ruleTile.m_TilingRules.Add(rule);
            }
        }
        ruleTile.m_DefaultColliderType = Tile.ColliderType.Grid;
        ruleTile.m_DefaultSprite = sprites[0];
        return ruleTile;
    }

    public static Sprite[] spritesFromTexture(Texture2D texture, string spritePath, string name, int width, int height) {
        if (texture.width % width != 0 || texture.height % height != 0) {
            Debug.Log("Invalid dimensions for texture");
            return null;
        }
        
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        
        spritePath += "/Sprites/";
        int rows = texture.height/height;
        int columns = texture.width/width;
        Sprite[] sprites = new Sprite[rows*columns];
        for (int y = 0; y < rows ; y++) {
            for (int x = 0; x < columns; x ++) {
                int index = y*columns+x;
                RuleVal ruleVal = indexToRule(index);

                Color[] pixels = texture.GetPixels(width*x,height*y,width,height);
                Texture2D spliteTexture = new Texture2D(width,height);
                spliteTexture.SetPixels(0,0,width,height,pixels);
                

                string spriteSavePath = spritePath + "S~" + name +"-" + index.ToString();
                byte[] pngBytes = spliteTexture.EncodeToPNG();
                File.WriteAllBytes(spriteSavePath+".png", pngBytes);
                AssetDatabase.Refresh();

                TextureImporter textureImporter = AssetImporter.GetAtPath(spriteSavePath + ".png") as TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spritePixelsPerUnit = 32;
                AssetDatabase.ImportAsset(spriteSavePath + ".png", ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath + ".png");
                sprites[index]=sprite1;
            }
        }
        return sprites;
    }

    public static BackgroundRuleTile backgroundRuleTileFrom24x24Texture(Texture2D texture, string spritePath, string name) {
    
        if (texture.width != 24 || texture.height != 24) {
            Debug.Log("Invalid dimensions for creating background ruletile. Must be 24 x 24");
            return null;
        }
        
        BackgroundRuleTile ruleTile = ScriptableObject.CreateInstance<BackgroundRuleTile>();
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();
        
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        Sprite[] sprites = new Sprite[16];
        spritePath += "/Sprites/";
        // 1 connected to tile, 0 not connected
        for (int up = 0; up <=1 ; up++) {
            for (int right = 0; right <= 1; right++) {
                for (int down = 0; down <= 1; down++) {
                    for (int left = 0; left <= 1; left++) {
                        Color[] pixels = texture.GetPixels(0,0,24,24);
                        if (down == 1) {
                            for (int x = 0; x < 24; x++) {
                                for (int y = 0; y < 4; y++) {
                                    pixels[y*24+x] = new Color(0,0,0,0);
                                }
                            }
                        }
                        if (up == 1) {
                            for (int x = 0; x < 24; x++) {
                                for (int y = 20; y < 24; y++) {
                                    pixels[y*24+x] = new Color(0,0,0,0);
                                }
                            }
                        }
                        if (left == 1) {
                            for (int x = 0; x < 4; x++) {
                                for (int y = 0; y < 24; y++) {
                                    pixels[y*24+x] = new Color(0,0,0,0);
                                }
                            }
                        }
                        if (right == 1) {
                            for (int x = 20; x < 24; x++) {
                                for (int y = 0; y < 24; y++) {
                                    pixels[y*24+x] = new Color(0,0,0,0);
                                }
                            }
                        }
                        Texture2D splitTexture = new Texture2D(24,24);
                        splitTexture.SetPixels(0,0,24,24,pixels);
                        string spriteName = name + "[";
                        if (up == 1) {
                            spriteName += "U";
                        }
                        if (right == 1) {
                            spriteName += "R";
                        }
                        if (down == 1) {
                            spriteName += "D";
                        }
                        if (left == 1) {
                            spriteName += "L";
                        }
                        spriteName += "]";
                        string spriteSavePath = spritePath + "S~" + spriteName;
                        byte[] pngBytes = splitTexture.EncodeToPNG();
                        File.WriteAllBytes(spriteSavePath+".png", pngBytes);
                        AssetDatabase.Refresh();

                        TextureImporter textureImporter = AssetImporter.GetAtPath(spriteSavePath + ".png") as TextureImporter;
                        textureImporter.textureType = TextureImporterType.Sprite;
                        textureImporter.spritePixelsPerUnit = 32;
                        AssetDatabase.ImportAsset(spriteSavePath + ".png", ImportAssetOptions.ForceUpdate);
                        AssetDatabase.Refresh();

                        Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath + ".png");
                        
                        SpriteEditorHelper.set(sprite1,false,false);
                        AssetDatabase.Refresh();
                        RuleTile.TilingRule rule = new RuleTile.TilingRule();
                        rule.m_ColliderType = Tile.ColliderType.Grid;
                        rule.m_Sprites = new Sprite[1];
                        rule.m_Sprites[0] = sprite1;
                        List<int> neighborRules = new List<int> {
                            0,2,0,2,2,0,2,0
                        };
                        if (up == 1) {
                            neighborRules[1] = 1;
                        }
                        if (left == 1) {
                            neighborRules[3] = 1;
                        }
                        if (right == 1) {
                            neighborRules[4] = 1;
                        }
                        if (down == 1) {
                            neighborRules[6] = 1;
                        }
                        // Default sprite is 24 x 24
                        if (up == 0 && right == 0 && left == 0 && down == 0) {
                            ruleTile.m_DefaultSprite = sprite1;
                        }
                        
                        // Collider is only grid if 16 x 16
                        if (up == 1 || right == 1 || left == 1 || down == 1) {
                            rule.m_ColliderType = Tile.ColliderType.Sprite;
                        }
                        rule.m_Neighbors = neighborRules;
                        ruleTile.m_TilingRules.Add(rule);
                    }
                }
            }
        }
        ruleTile.m_DefaultColliderType = Tile.ColliderType.Grid;
        return ruleTile;
    }

    public static Tile[] fluidTilesFromSprite(Texture2D texture, string spritePath, string tileName, bool inverted) {
        int TILES_TO_CREATE = 8;
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        Tile[] tiles = new Tile[8];
        spritePath += "/Sprites/";
        
        for (int i = 0; i < TILES_TO_CREATE; i++) {
            int yMin = inverted ? 0 : 16-2*(TILES_TO_CREATE-(i+1));
            int yMax = inverted ? 2*(TILES_TO_CREATE-(i+1)) : 16;
            Debug.Log(yMin);
            Color[] pixels = texture.GetPixels(0,0,16,16);
            if (i != TILES_TO_CREATE-1) {
                for (int x = 0; x < 16; x++) {
                    for (int y = yMin; y < yMax; y++) {
                        pixels[y*16+x] = new Color(0,0,0,0);
                    }
                }
            }
            Texture2D dividedTexture = new Texture2D(16,16);
            dividedTexture.SetPixels(0,0,16,16,pixels);
            string spriteName = tileName + "[" + i + "]";
            string spriteSavePath = spritePath + "S~" + spriteName;
            byte[] pngBytes = dividedTexture.EncodeToPNG();
            File.WriteAllBytes(spriteSavePath+".png", pngBytes);
            AssetDatabase.Refresh();

            TextureImporter textureImporter = AssetImporter.GetAtPath(spriteSavePath + ".png") as TextureImporter;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spritePixelsPerUnit = 32;
            AssetDatabase.ImportAsset(spriteSavePath + ".png", ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spriteSavePath + ".png");
            SpriteEditorHelper.set(sprite1,false,false);
            AssetDatabase.Refresh();

            TileColliderType tileColliderType = i == TILES_TO_CREATE -1 ? TileColliderType.Tile : TileColliderType.Sprite;
            
            StandardTile tile = ItemEditorFactory.standardTileCreator(sprite1,tileColliderType);
            tile.id = ItemEditorFactory.formatId(tileName);
            tile.name = tileName + "[" + i + "]";
            AssetDatabase.CreateAsset(tile,spritePath + tile.name + ".asset");
            AssetDatabase.Refresh();
        }
        return tiles;
    }

    private static RuleVal indexToRule(int index) {
        switch(index) {
        case 0:
            return RuleVal.Empty;
        case 1:
            return RuleVal.U;
        case 2:
            return RuleVal.R;
        case 3:
            return RuleVal.D;
        case 4:
            return RuleVal.L;
        case 5:
            return RuleVal.UL;
        case 6:
            return RuleVal.UR;
        case 7:
            return RuleVal.RD;
        case 8:
            return RuleVal.DL;
        case 9:
            return RuleVal.LR;
        case 10:
            return RuleVal.UD;
        case 11:
            return RuleVal.URD;
        case 12:
            return RuleVal.RDL;
        case 13:
            return RuleVal.UDL;
        case 14:
            return RuleVal.URL;
        case 15:
            return RuleVal.URDL;
        default:
            Debug.LogError("Rule Index above 15");
            return RuleVal.Null;
        }
    }

    private enum RuleVal {
        Null,
        Empty,
        U,
        R,
        D,
        L,
        UL,
        UR,
        RD,
        DL,
        LR,
        UD,
        URD,
        RDL,
        UDL,
        URL,
        URDL,
    }
}

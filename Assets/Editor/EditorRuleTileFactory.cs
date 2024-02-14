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
    public static RuleTile ruleTilefrom64x64Texture(Texture2D texture, string spritePath, string name) {
        Vector2 textureSize = new Vector2(texture.width,texture.height);
        Vector2Int adjustedTextureSize = new Vector2Int(Mathf.FloorToInt(textureSize.x/16f),Mathf.FloorToInt(textureSize.y/16f));
        if (adjustedTextureSize.x != 4 || adjustedTextureSize.y != 4) {
            Debug.Log("Invalid dimensions for creating ruletile " + adjustedTextureSize + ". Must be 4 x 4");
        }
        
        RuleTile ruleTile = ScriptableObject.CreateInstance<RuleTile>();
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

    public static Sprite[] spritesFromTexture(Texture2D texture, string spritePath, string name) {
        if (texture.width % 16 != 0 || texture.height % 16 != 0) {
            Debug.Log("Invalid dimensions for texture");
            return null;
        }
        
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        
        spritePath += "/Sprites/";
        int rows = texture.height/16;
        int columns = texture.width/16;
        Sprite[] sprites = new Sprite[rows*columns];
        for (int y = 0; y < rows ; y++) {
            for (int x = 0; x < columns; x ++) {
                int index = y*columns+x;
                RuleVal ruleVal = indexToRule(index);

                Color[] pixels = texture.GetPixels(16*x,16*y,16,16);
                Texture2D spliteTexture = new Texture2D(16,16);
                spliteTexture.SetPixels(0,0,16,16,pixels);
                

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

    public static RuleTile backgroundRuleTileFrom24x24Texture(Texture2D texture, string spritePath, string name) {
    
        if (texture.width != 24 || texture.height != 24) {
            Debug.Log("Invalid dimensions for creating background ruletile. Must be 24 x 24");
            return null;
        }
        
        RuleTile ruleTile = ScriptableObject.CreateInstance<RuleTile>();
        ruleTile.m_TilingRules = new List<RuleTile.TilingRule>();
        
        AssetDatabase.CreateFolder(spritePath, "Sprites");
        Sprite[] sprites = new Sprite[16];
        spritePath += "/Sprites/";
        // 1 include, 0 exclude
        for (int up = 0; up <=1 ; up++) {
            for (int right = 0; right <= 1; right++) {
                for (int down = 0; down <= 1; down++) {
                    for (int left = 0; left <= 1; left++) {
                        
                        int copyXStart; int copyYStart; int copyXEnd; int copyYEnd;
                        
                        copyXStart = (right == 1) ? 0 : 4;
                        copyYStart = (up == 1) ? 0 : 4;
                        copyXEnd = (left == 1) ? 24-copyXStart : 20-copyXStart;
                        copyYEnd = (down == 1) ? 24-copyYStart : 20-copyYStart; 
                        
                        Color[] pixels = texture.GetPixels(copyXStart,copyYStart,copyXEnd,copyYEnd);


                        int newXSize = 16; int newYSize = 16;
                        if (left == 1 || right == 1) {
                            newXSize = 24;
                        }
                        if (up == 1 || down == 1) {
                            newYSize = 24;
                        }
                        //Debug.Log("U:" + up + "D:" + down + "R:" + right + "L" + left);
                        Texture2D splitTexture = new Texture2D(newXSize,newYSize);
                        // set all values to empty
                        for (int i = 0; i < splitTexture.width; i++)
                        {
                            for (int j = 0; j < splitTexture.height; j++)
                            {
                                splitTexture.SetPixel(i, j, Color.clear);
                                //splitTexture.SetPixel(i, j, Color.white);
                            }
                        }
                        int placeXStart=0; int placeYStart=0; int placeXWidth=16; int placeYHeight=16;
                        if (left == 1 && right == 1) {
                            placeXStart = 0; placeXWidth = 24;
                        } else if (left == 1 && right == 0) {
                            placeXStart = 0; placeXWidth = 20;
                        } else if (left == 0 && right == 1) {
                            placeXStart = 4; placeXWidth = 20;
                        }
                        if (up == 1 && down == 1) {
                            placeYStart = 0; placeYHeight = 24;
                        } else if (up == 1 && down == 0) {
                            placeYStart = 4; placeYHeight = 20;
                        } else if (up == 0 && down == 1) {
                            placeYStart = 0; placeYHeight = 20;
                        }
                        //Debug.Log(placeYStart + "," + placeYHeight);
                        splitTexture.SetPixels(placeXStart,placeYStart,placeXWidth,placeYHeight,pixels);

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

                        RuleTile.TilingRule rule = new RuleTile.TilingRule();
                        rule.m_ColliderType = Tile.ColliderType.Grid;
                        rule.m_Sprites = new Sprite[1];
                        rule.m_Sprites[0] = sprite1;
                        List<int> neighborRules = new List<int> {
                            0,2,0,2,2,0,2,0
                        };
                        if (up == 0) {
                            neighborRules[1] = 1;
                        }
                        if (left == 0) {
                            neighborRules[3] = 1;
                        }
                        if (right == 0) {
                            neighborRules[4] = 1;
                        }
                        if (down == 0) {
                            neighborRules[6] = 1;
                        }
                        // Default sprite is 24 x 24
                        if (up == 1 && right == 1 && left == 1 && down == 1) {
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

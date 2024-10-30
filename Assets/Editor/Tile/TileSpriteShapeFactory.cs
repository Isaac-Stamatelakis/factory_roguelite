using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class TileSpriteShapeFactory 
{
    public static Sprite[] generateSprites(Texture2D texture, string path, string name, Color[] shape, int rotation) {
        if (shape.Length != 256) {
            Debug.LogError("Invalid shape dimensions");
            return null;
        }
        int width = texture.width/16; 
        int height = texture.height/16;
        Sprite[] sprites = new Sprite[width*height];
        string spritePath = Path.Combine(path,name);
        Color transparentColor = new Color(0,0,0,0);
        for (int sectionX = 0; sectionX < width; sectionX ++) {
            for (int sectionY = 0; sectionY < height; sectionY++) {
                Color[] pixels = texture.GetPixels((sectionX)*16,(sectionY)*16,16,16);
                for (int x = 0; x < 16; x++) {
                    for (int y = 0; y < 16; y++) {
                        int index = getIndexFromRotation(x,y,rotation);
                        if (shape[index].a == 0) {
                            pixels[y*16+x] = transparentColor;
                        }
                    }
                }
                string savePath = Path.Combine(spritePath,$"{name}[{sectionX},{sectionY}]");
                sprites[sectionX+height*sectionY] = pixelsToSprite(pixels,savePath,16,16);
            }
        }
        return sprites;
    }

    private static int getIndexFromRotation(int x, int y, int rotation) {
    switch (rotation) {
        case 0: // 0
            return y * 16 + x;
        case 1: // 90
            return (15 - x) * 16 + y;
        case 2: // 180
            return (15 - y) * 16 + (15 - x);
        case 3: // 270
             return x * 16 + (15 - y);
        default:
            throw new Exception($"Rotation {rotation} is not valid");
    }
}

    private static Color[] getSlabPixels(Color[] pixels, int rotation) {
        int startX, endX, startY, endY;
        switch (rotation) {
            case 0:  // No rotation
                startX = 0; endX = 16;  // All X coordinates
                startY = 8; endY = 16;  // Bottom half
                break;
            case 1:  // 90 degrees clockwise
                startX = 0; endX = 8;   // Left half
                startY = 0; endY = 16;  // All Y coordinates
                break;
            case 2:  // 180 degrees
                startX = 0; endX = 16;  // All X coordinates
                startY = 0; endY = 8;   // Top half
                break;
            case 3:  // 270 degrees clockwise
                startX = 8; endX = 16;  // Right half
                startY = 0; endY = 16;  // All Y coordinates
                
                break;
            default:
                throw new ArgumentException("Invalid rotation value. Expected 0, 1, 2, or 3 degrees.");
        }
        for (int x = startX; x < endX; x++) {
            for (int y = startY; y < endY; y++) {
                pixels[y*16+x] = new Color(0,0,0,0);
            }
        }
        return pixels;
    }

    private static Color[] getSlantPixels(Color[] pixels, int rotation) {
        int width = 16;
        switch (rotation) {
            case 0: // Original bottom-left triangle
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < x; y++) {
                        int correctedX = width - 1 - x;
                        int correctedY = width - 1 - y;
                        pixels[correctedY * width + correctedX] = new Color(0, 0, 0, 0);
                    }
                }
                break;
            case 1: // Top-left triangle
                for (int x = 0; x < width; x++) {
                    for (int y = x + 1; y < width; y++) {
                        int correctedX = width - 1 - x;
                        int correctedY = width - 1 - y;
                        pixels[correctedY * width + correctedX] = new Color(0, 0, 0, 0);
                    }
                }
                break;
            case 2: // Bottom-right triangle
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < width - x - 1; y++) {
                        pixels[y * width + x] = new Color(0, 0, 0, 0);
                    }
                }
                break;
            case 3: // Top-right triangle
                for (int x = 0; x < width; x++) {
                    for (int y = width - x; y < width; y++) {
                        pixels[y * width + x] = new Color(0, 0, 0, 0);
                    }
                }
                break;
            default:
                throw new ArgumentException("Invalid orientation value. Expected 0, 1, 2, or 3.");
        }
        return pixels;
    }

    public static Sprite[] generateStandardSlanted(Texture2D texture, string path, string name = "slant") {
        int width = texture.width/16; 
        int height = texture.height/16;
        Sprite[] sprites = new Sprite[width*height];
        for (int sectionX = 0; sectionX < width; sectionX ++) {
            for (int sectionY = 0; sectionY < height; sectionY++) {
                Color[] pixels = texture.GetPixels((sectionX)*16,(sectionY)*16,16,16);
                
                sprites[sectionX+height*sectionY] = pixelsToSprite(pixels,path + $"{name}[{sectionX},{sectionY}]",16,16);
            }
        }
        return sprites;
    }

    public static Sprite[] generateSpritesFromShapeSheet(Texture2D texture, string path, Sprite[] shapes) {
        Sprite[] sprites = new Sprite[shapes.Length];
        for (int i = 0; i < shapes.Length; i++) {
            Color[] pixels = texture.GetPixels(0,0,16,16);
            Rect textureRect = shapes[i].textureRect;
            Color[] shapePixels = shapes[i].texture.GetPixels(
                (int)textureRect.x, 
                (int)textureRect.y, 
                (int)textureRect.width, 
                (int)textureRect.height
            );
            for (int x = 0; x < 16; x++) {
                for (int y = 0; y < 16; y++) {
                    Color shapePixel = shapePixels[y*16+x];
                    if (shapePixel.a != 0) {
                        continue;
                    }
                    pixels[y*16+x] = new Color(0,0,0,0);
                }
            }
            string spritePath = path + i;
            sprites[i] = pixelsToSprite(pixels,spritePath,16,16);
        }
        return sprites;
    }

    public static Sprite pixelsToSprite(Color[,] pixels, string spritePath) {
        Color[] pixelArray = new Color[pixels.GetLength(0)*pixels.GetLength(1)];
        for (int x = 0; x < pixels.GetLength(0); x++) {
            for (int y = 0; y < pixels.GetLength(1); y++) {
                pixelArray[x+y*pixelArray.GetLength(0)] = pixels[x,y];
            }
        }
        return pixelsToSprite(pixelArray,spritePath,pixels.GetLength(0),pixels.GetLength(1));
        
    }
    public static Sprite pixelsToSprite(Color[] pixels, string spritePath, int width, int height) {
        Texture2D newTexture = new Texture2D(width,height);
        newTexture.SetPixels(0,0,width,height,pixels);
        byte[] pngBytes = newTexture.EncodeToPNG();
        File.WriteAllBytes(spritePath+".png", pngBytes);
        AssetDatabase.Refresh();
        Debug.Log(spritePath + ".png");
        TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath + ".png") as TextureImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spritePixelsPerUnit = 32;
        AssetDatabase.ImportAsset(spritePath + ".png", ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + ".png");
        SpriteEditorHelper.set(sprite,false,false);
        AssetDatabase.Refresh();
        return sprite;
    }
}

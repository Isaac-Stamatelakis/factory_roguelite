using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class TileSpriteShapeFactory 
{
    public static Sprite generateSlab(Texture2D texture, string path, string name = "slab") {
        Color[] pixels = texture.GetPixels(0,0,16,16);
        for (int x = 0; x < 16; x++) {
            for (int y = 8; y < 16; y++) {
                pixels[y*16+x] = new Color(0,0,0,0);
            }
        }
        string spritePath = path + "slab";
        return pixelsToSprite(pixels,spritePath);
    }

    public static Sprite generateStandardSlanted(Texture2D texture, string path, string name = "slant") {
        Color[] pixels = texture.GetPixels(0,0,16,16);
        for (int x = 0; x < 16; x++) {
            for (int y = 0; y < x; y++) {
                int correctedX = 15-x;
                int correctedY = 15-y;
                pixels[correctedY*16+correctedX] = new Color(0,0,0,0);
            }
        }
        string spritePath = path + "slant";
        return pixelsToSprite(pixels,spritePath);
    }

    public static Sprite[] generateSpritesFromShapeSheet(Texture2D texture, string path,Sprite[] shapes) {
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
            sprites[i] = pixelsToSprite(pixels,spritePath);
        }
        return sprites;
    }

    private static Sprite pixelsToSprite(Color[] pixels, string spritePath) {
        Texture2D newTexture = new Texture2D(16,16);
        newTexture.SetPixels(0,0,16,16,pixels);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SpritePostProcess : AssetPostprocessor
{
    bool enabled = false;
    void OnPreprocessTexture()
    {
        if (enabled) {
            TextureImporter textureImporter = (TextureImporter)assetImporter;

            if (textureImporter.textureType == TextureImporterType.Sprite)
            {
                // Set the desired import settings here
                textureImporter.isReadable = true; // Read/Write Enabled
                textureImporter.spritePixelsPerUnit = 32; // Pixels Per Unit
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed; // Compression None
                textureImporter.filterMode = FilterMode.Point;
            }
        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class OutlineTileGenerator
{
    private Color[,] pixels;
    private string folder;
    private string name;
    private bool ruleTile;

    public OutlineTileGenerator(Color[,] pixels, string folder, string name, bool ruleTile)
    {
        this.pixels = pixels;
        this.folder = folder;
        this.name = name;
        this.ruleTile = ruleTile;
    }

    public TileBase generate() {
        if (!ruleTile) {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            string spritePath = folder + name;
            Sprite sprite = TileSpriteShapeFactory.pixelsToSprite(pixels,spritePath);
            tile.sprite = sprite;
            AssetDatabase.CreateAsset(tile,folder + name + ".asset");
            return tile;
        }
        return null;
        
    }

    /// <summary>
    /// This returns the directions that the outline can connect to 
    /// </summary>
    private List<Vector2Int> getRuleDirections(Color[,] pixels) {
        return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public static class TileItemEditorFactory
{
    public static void generateTileItem(string tileName, TileBase tile, bool createFolder = true,string savePath = "Assets/EditorCreations/") {
        string path = savePath + tileName + "/";
        
        if (createFolder) {
            if (AssetDatabase.IsValidFolder(path)) {
                Debug.LogWarning("Replaced existing content at " + path);
                Directory.Delete(path);
                return;
            }
            AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        }
        
        
        AssetDatabase.CreateAsset(tile, path + "T~" +tileName + ".asset");

        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.name = tileName;
        tileItem.tile = tile;
        tileItem.id = tileName;
        tileItem.tileType = TileType.Background;
        tileItem.id = tileItem.id.ToLower().Replace(" ","_");
        if (tile is not IIDTile idTile) {
            Debug.LogWarning("Tile generated for  " + tileName + " is not IIDTile");
        } else {
            idTile.setID(tileItem.id);
        }
        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
        Debug.Log("Background Tile Created at Path: " + path);
    }

    public static StandardTile standardTileCreator(Sprite sprite) {
        StandardTile tile = ScriptableObject.CreateInstance<StandardTile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        Vector2Int spriteSize = Global.getSpriteSize(sprite);
        Matrix4x4 tileTransform = tile.transform;
        if (spriteSize.x % 2 == 0) {
            tileTransform.m03 = 0.25f;
        }
        if (spriteSize.y % 2 == 0) {
            tileTransform.m13 = 0.25f;
        }
        tile.transform = tileTransform;
        return tile;
        
        
    }
}

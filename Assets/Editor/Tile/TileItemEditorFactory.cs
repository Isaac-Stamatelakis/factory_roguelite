using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using TileEntityModule;

public static class TileItemEditorFactory
{
    public static void generateTileItem(string tileName, TileBase tile, TileType tileType, bool createFolder = true, string savePath = "Assets/EditorCreations/", TileEntity tileEntity = null) {
        string path = savePath + tileName + "/";
        
        if (createFolder) {
            createDirectory(tileName,savePath);
        }
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.id = tileName;
        tileItem.tileType = tileType;
        tileItem.id = tileItem.id.ToLower().Replace(" ","_");
        tileItem.name = tileName;
        tileItem.tile = tile;
        if (tileEntity != null) {
            tileItem.tileEntity = tileEntity;
        }
        
        if (tile is not IIDTile idTile) {
            Debug.LogWarning("Tile generated for  " + tileName + " is not IIDTile");
        } else {
            idTile.setID(tileItem.id);
        }
        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
        Debug.Log("Background Tile Created at Path: " + path);
    }

    public static void createDirectory(string tileName, string savePath = "Assets/EditorCreations/") {
        string path = savePath + tileName + "/";
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
    }
    public static void saveTile(TileBase tileBase, string tileName, string addition = "",string path = "Assets/EditorCreations/") {
        string savePath = path + tileName + "/";
        AssetDatabase.CreateAsset(tileBase, savePath + "T~" +tileName + addition + ".asset");
    }

    public static void saveTileEntity(TileEntity tileEntity, string tileName, string path = "Assets/EditorCreations/") {
        string savePath = path + tileName + "/";
        AssetDatabase.CreateAsset(tileEntity, savePath + "E~" +tileName + ".asset");
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

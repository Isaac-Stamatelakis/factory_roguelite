using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using TileEntity;
using Tiles;
using Tiles.CustomTiles.IdTiles;

public enum TileColliderType {
    Tile,
    Sprite
}
public static class ItemEditorFactory
{
    [Obsolete("This function is ugly don't want to change and break stuff though.")] 
    public static TileItem GeneratedTileItem(string tileName, TileBase tile, TileType tileType, bool createFolder = true, string savePath = "Assets/EditorCreations/", TileEntityObject tileEntity = null, TileBase outline = null) {
        string path = savePath + tileName + "/";
        if (createFolder) {
            CreateDirectory(tileName,savePath);
        }
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.tileType = tileType;
        tileItem.id = formatId(tileName);
        tileItem.name = tileName;
        tileItem.tile = tile;
        tileItem.outline = outline;
        
        tileItem.tileEntity = tileEntity;
        
        AssetDatabase.CreateAsset(tileItem, path + tileItem.name + ".asset");
        Debug.Log("Tile Created at Path: " + path);
        AssetDatabase.Refresh();
        
        return tileItem;
    }

    public static TileItem GenerateTileItem(string name, string folderPath, TileBase tileBase, TileType tileType,
        TileBase outline)
    {
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.tileType = tileType;
        tileItem.id = formatId(name);
        tileItem.name = name;
        tileItem.tile = tileBase;
        tileItem.outline = outline;
        string savePath = Path.Combine(folderPath, tileItem.name + ".asset");
        AssetDatabase.CreateAsset(tileItem, savePath);
        Debug.Log("Tile Created at Path: " + savePath);
        AssetDatabase.Refresh();
        
        return tileItem;
    }

    public static string formatId(string tileName) {
        return tileName.ToLower().Replace(" ","_");
    }
    public static void CreateDirectory(string tileName, string savePath = "Assets/EditorCreations/") {
        string path = savePath + tileName + "/";
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
    }
    public static void saveTileWithName(TileBase tileBase, string tileName, string addition = "",string path = "Assets/EditorCreations/") {
        if (!path.EndsWith("/")) {
            path = path + '/';
        }
        string savePath = Path.Combine(path+tileName,$"T~{tileName}{addition}.asset");
        Debug.Log(savePath);
        AssetDatabase.CreateAsset(tileBase,savePath);
        AssetDatabase.Refresh();
    }

    public static void saveTile(TileBase tileBase, string path = "Assets/EditorCreations/") {
        AssetDatabase.CreateAsset(tileBase,path + tileBase.name + ".asset");
        AssetDatabase.Refresh();
    }

    

    public static Tile createTile(Sprite sprite, string tileName, string path) {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.name = tileName;
        saveTile(tile,path);
        return tile;
    }


    public static void saveTileEntity(TileEntityObject tileEntity, string tileName, string path = "Assets/EditorCreations/") {
        string savePath = path + tileName + "/";
        AssetDatabase.CreateAsset(tileEntity, savePath + "E~" +tileName + ".asset");
    }
    public static Tile StandardTileCreator(Sprite sprite,TileColliderType colliderType) {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        if (colliderType == TileColliderType.Tile) {
            tile.colliderType = Tile.ColliderType.Grid;
        } else if (colliderType == TileColliderType.Sprite) {
            tile.colliderType = Tile.ColliderType.Sprite;
        }
        
        setTileTransformOffset(sprite,tile);
        
        return tile;
    }

    public static void setTileTransformOffset(Sprite sprite, Tile tile) {
        Vector2Int spriteSize = Global.GetSpriteSize(sprite);
        Matrix4x4 tileTransform = tile.transform;
        if (spriteSize.x % 2 == 0) {
            tileTransform.m03 = 0.25f;
        }
        if (spriteSize.y % 2 == 0) {
            tileTransform.m13 = 0.25f;
        }
        tile.transform = tileTransform;
    }
}

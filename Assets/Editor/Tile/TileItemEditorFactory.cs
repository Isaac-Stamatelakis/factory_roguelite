using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using TileEntityModule;
 public enum TileColliderType {
    Tile,
    Sprite
}
public static class ItemEditorFactory
{
    public static void generateTileItem(string tileName, TileBase tile, TileType tileType, bool createFolder = true, string savePath = "Assets/EditorCreations/", TileEntity tileEntity = null) {
        string path = savePath + tileName + "/";
        
        if (createFolder) {
            createDirectory(tileName,savePath);
        }
        TileItem tileItem = ScriptableObject.CreateInstance<TileItem>();
        tileItem.tileType = tileType;
        tileItem.id = formatId(tileName);
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
        Debug.Log("Tile Created at Path: " + path);
    }

    public static string formatId(string tileName) {
        return tileName.ToLower().Replace(" ","_");
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
        AssetDatabase.Refresh();
    }


    public static void saveTileEntity(TileEntity tileEntity, string tileName, string path = "Assets/EditorCreations/") {
        string savePath = path + tileName + "/";
        AssetDatabase.CreateAsset(tileEntity, savePath + "E~" +tileName + ".asset");
    }
    public static StandardTile standardTileCreator(Sprite sprite,TileColliderType colliderType) {
        StandardTile tile = ScriptableObject.CreateInstance<StandardTile>();
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
        Vector2Int spriteSize = Global.getSpriteSize(sprite);
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

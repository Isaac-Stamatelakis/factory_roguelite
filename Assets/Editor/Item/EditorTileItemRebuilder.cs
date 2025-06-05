using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EditorTileItemRebuilder
{
    private string tileName;
    private TileItem tileItem;
    

    public void SearchByTileName(string name)
    {
        tileName = name;
        string tileId = tileName.ToLower().Replace(" ", "_");
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(TileItem));
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TileItem searchItem = AssetDatabase.LoadAssetAtPath<TileItem>(path);
            if (!string.Equals(searchItem?.id, tileId)) continue;
            tileItem = searchItem;
            return;
        }

        tileItem = null;
    }

    public bool Found()
    {
        return tileItem;
    }

    public string GetItemPath()
    {
        return !Found() ? string.Empty : AssetDatabase.GetAssetPath(tileItem);
    }

    public void DeleteOldTileAssets()
    {
        string path = AssetDatabase.GetAssetPath(tileItem);
        string itemFolderPath = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(itemFolderPath))
        {
            Debug.LogError("Could not find item path");
            return;
        }
        
        TileBase oldTile = tileItem.tile;
        if (oldTile)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(oldTile));
        }
        
        const string SPRITE_FOLDER_NAME = "Sprites";
        string spriteFolderPath = Path.Combine(itemFolderPath, SPRITE_FOLDER_NAME);
        if (AssetDatabase.IsValidFolder(spriteFolderPath)) return;
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { spriteFolderPath });
        
        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(assetPath);
        }
        AssetDatabase.Refresh();
    }
    

    public void DisplayGUI(Action onClick, ref string name)
    {
        if (GUILayout.Button("Search For Item"))
        {
            SearchByTileName(name);
        }
        bool found = Found();
        GUI.enabled = found;
        Color defaultColor = GUI.color;
        if (found)
        {
            GUI.color = Color.green;
        }
        if (GUILayout.Button("Rebuild"))
        {
            onClick.Invoke();
        }

        GUI.color = defaultColor;
        
        GUI.enabled = true;
    }

    public void ReplaceTile(TileBase tileBase)
    {
        tileItem.tile = tileBase;
        EditorUtility.SetDirty(tileItem);
        AssetDatabase.SaveAssetIfDirty(tileItem);
        AssetDatabase.Refresh();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Tilemaps;
using Tiles;

public class TileOutlineApplierWindow : EditorWindow {
    private OutlineValues outlineValues;
    [MenuItem("Tools/Item Constructors/Tile/Outline/Applier")]
    public static void ShowWindow()
    {
        TileOutlineApplierWindow window = (TileOutlineApplierWindow)EditorWindow.GetWindow(typeof(TileOutlineApplierWindow));
        window.titleContent = new GUIContent("Outline Applier");
    }
    void OnGUI()
    {
        GUI.enabled = false;
        GUILayout.TextArea("Assigns respective outline tile to all TileItems of type Block");
        GUI.enabled = true;
        if (GUILayout.Button("Sync Outlines"))
        {
            Apply();
        }
    }

    void Apply()
    {
        outlineValues = new OutlineValues();
        int count = 0;
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(TileItem));
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TileItem tileItem = AssetDatabase.LoadAssetAtPath<TileItem>(path);
            if (!tileItem || tileItem.tileType != TileType.Block || !tileItem.tile) continue;
            
            if (path.StartsWith("Assets/EditorCreations/")) 
                continue;
            
            TileBase outline = outlineValues.FromTile(tileItem.tile);
            if (ReferenceEquals(tileItem.tile,outline)) continue; // Skip so we can get accurate count
            tileItem.outline = outline;
            count++;
        }
        Debug.Log($"Assigned Outlines to {count} Tile Items");
    }
    
}

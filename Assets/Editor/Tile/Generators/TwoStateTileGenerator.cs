using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tiles;
using Tiles.CustomTiles.IdTiles;
using Tiles.CustomTiles.StateTiles.Instances;

public class TwoStateTileGenerator : EditorWindow

{
    private Sprite inactive;
    private Sprite active;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/Special/TwoState")]
    // Start is called before the first frame update
    public static void ShowWindow()
    {
        TwoStateTileGenerator window = (TwoStateTileGenerator)EditorWindow.GetWindow(typeof(TwoStateTileGenerator));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        inactive = EditorGUILayout.ObjectField("Inactive", inactive, typeof(Sprite), true) as Sprite;
        active = EditorGUILayout.ObjectField("Active", active, typeof(Sprite), true) as Sprite;
        
        GUILayout.FlexibleSpace();
    
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
    }
    void createTileItem()
    {
        ItemEditorFactory.createDirectory(tileName);

        StandardTile inactiveTile = ItemEditorFactory.standardTileCreator(inactive,TileColliderType.Tile);
        ItemEditorFactory.saveTileWithName(inactiveTile,tileName,"Inactive");

        StandardTile activeTile = ItemEditorFactory.standardTileCreator(active,TileColliderType.Sprite);
        ItemEditorFactory.saveTileWithName(activeTile,tileName,"Active");

       
        TwoStateTile twoStateTile = ScriptableObject.CreateInstance<TwoStateTile>();
        twoStateTile.activeTile = activeTile;
        twoStateTile.inactiveTile = inactiveTile;
        twoStateTile.setID(tileName.ToLower().Replace(" ", "_"));
        ItemEditorFactory.saveTileWithName(twoStateTile,tileName);
#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: twoStateTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: null
        );
#pragma warning restore CS0618 // Type or member is obsolete
        AssetDatabase.Refresh();
    }
}

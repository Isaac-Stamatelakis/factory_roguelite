using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tiles;

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
        TileItemEditorFactory.createDirectory(tileName);

        StandardTile inactiveTile = TileItemEditorFactory.standardTileCreator(inactive,TileColliderType.Tile);
        TileItemEditorFactory.saveTile(inactiveTile,tileName,"Inactive");

        StandardTile activeTile = TileItemEditorFactory.standardTileCreator(active,TileColliderType.Sprite);
        TileItemEditorFactory.saveTile(activeTile,tileName,"Active");

       
        TwoStateTile twoStateTile = ScriptableObject.CreateInstance<TwoStateTile>();
        twoStateTile.activeTile = activeTile;
        twoStateTile.inactiveTile = inactiveTile;
        twoStateTile.setID(tileName.ToLower().Replace(" ", "_"));
        TileItemEditorFactory.saveTile(twoStateTile,tileName);
        TileItemEditorFactory.generateTileItem(
            tileName: tileName,
            tile: twoStateTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: null
        );
        AssetDatabase.Refresh();
    }
}

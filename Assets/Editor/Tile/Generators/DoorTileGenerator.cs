using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles;
using TileEntity.Instances;
using Tiles.CustomTiles.IdTiles;

public class DoorTileGenerator : EditorWindow {
    private Sprite leftClosed;
    private Sprite leftOpen;
    private Sprite rightClosed;
    private Sprite rightOpen;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Tile/Special/Door")]
    public static void ShowWindow()
    {
        DoorTileGenerator window = (DoorTileGenerator)EditorWindow.GetWindow(typeof(DoorTileGenerator));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        leftClosed = EditorGUILayout.ObjectField("Left Closed", leftClosed, typeof(Sprite), true) as Sprite;
        leftOpen = EditorGUILayout.ObjectField("Left Open", leftOpen, typeof(Sprite), true) as Sprite;
        rightClosed = EditorGUILayout.ObjectField("Right Closed", rightClosed, typeof(Sprite), true) as Sprite;
        rightOpen = EditorGUILayout.ObjectField("Right Open", rightOpen, typeof(Sprite), true) as Sprite;
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
        ItemEditorFactory.CreateDirectory(tileName);

        Tile leftClosedTile = ItemEditorFactory.StandardTileCreator(leftClosed,TileColliderType.Sprite);
        ItemEditorFactory.saveTileWithName(leftClosedTile,tileName,"LeftClosed");

        Tile leftOpenTile = ItemEditorFactory.StandardTileCreator(leftOpen,TileColliderType.Sprite);
        ItemEditorFactory.saveTileWithName(leftOpenTile,tileName,"LeftOpen");

        Tile rightClosedTile = ItemEditorFactory.StandardTileCreator(rightClosed,TileColliderType.Sprite);
        ItemEditorFactory.saveTileWithName(rightClosedTile,tileName,"RightClosed");

        Tile rightOpenTile = ItemEditorFactory.StandardTileCreator(rightOpen,TileColliderType.Sprite);
        ItemEditorFactory.saveTileWithName(rightOpenTile,tileName,"RightOpen");

        IMousePositionStateDoorTile doorTile = ScriptableObject.CreateInstance<IMousePositionStateDoorTile>();
        doorTile.left = leftClosedTile;
        doorTile.leftOpen = leftOpenTile;
        doorTile.right = rightClosedTile;
        doorTile.rightOpen = rightOpenTile;
        

        Door door = ScriptableObject.CreateInstance<Door>();
        ItemEditorFactory.saveTileEntity(door,tileName);

        ItemEditorFactory.saveTileWithName(doorTile,tileName);

#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: doorTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: door
        );
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

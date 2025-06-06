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
        ItemEditorFactory.SaveTileWithName(leftClosedTile,tileName,"LeftClosed");

        Tile leftOpenTile = ItemEditorFactory.StandardTileCreator(leftOpen,TileColliderType.Sprite);
        ItemEditorFactory.SaveTileWithName(leftOpenTile,tileName,"LeftOpen");

        Tile rightClosedTile = ItemEditorFactory.StandardTileCreator(rightClosed,TileColliderType.Sprite);
        ItemEditorFactory.SaveTileWithName(rightClosedTile,tileName,"RightClosed");

        Tile rightOpenTile = ItemEditorFactory.StandardTileCreator(rightOpen,TileColliderType.Sprite);
        ItemEditorFactory.SaveTileWithName(rightOpenTile,tileName,"RightOpen");

        IMousePositionStateDoorTile doorDoorTile = ScriptableObject.CreateInstance<IMousePositionStateDoorTile>();
        doorDoorTile.left = leftClosedTile;
        doorDoorTile.leftOpen = leftOpenTile;
        doorDoorTile.right = rightClosedTile;
        doorDoorTile.rightOpen = rightOpenTile;
        

        Door door = ScriptableObject.CreateInstance<Door>();
        ItemEditorFactory.saveTileEntity(door,tileName);

        ItemEditorFactory.SaveTileWithName(doorDoorTile,tileName);

#pragma warning disable CS0618 // Type or member is obsolete
        ItemEditorFactory.GeneratedTileItem(
            tileName: tileName,
            tile: doorDoorTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: door
        );
#pragma warning restore CS0618 // Type or member is obsolete
    }
}

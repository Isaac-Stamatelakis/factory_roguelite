using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles;
using TileEntityModule.Instances;

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
        TileItemEditorFactory.createDirectory(tileName);

        StandardTile leftClosedTile = TileItemEditorFactory.standardTileCreator(leftClosed);
        TileItemEditorFactory.saveTile(leftClosedTile,tileName,"LeftClosed");

        StandardTile leftOpenTile = TileItemEditorFactory.standardTileCreator(leftOpen);
        TileItemEditorFactory.saveTile(leftOpenTile,tileName,"LeftOpen");

        StandardTile rightClosedTile = TileItemEditorFactory.standardTileCreator(rightClosed);
        TileItemEditorFactory.saveTile(rightClosedTile,tileName,"RightClosed");

        StandardTile rightOpenTile = TileItemEditorFactory.standardTileCreator(rightOpen);
        TileItemEditorFactory.saveTile(rightOpenTile,tileName,"RightOpen");

        RestrictedDoorTile doorTile = ScriptableObject.CreateInstance<RestrictedDoorTile>();
        doorTile.left = leftClosedTile;
        doorTile.leftOpen = leftOpenTile;
        doorTile.right = rightClosedTile;
        doorTile.rightOpen = rightOpenTile;
        doorTile.setID(tileName.ToLower().Replace(" ", "_"));

        Door door = ScriptableObject.CreateInstance<Door>();
        TileItemEditorFactory.saveTileEntity(door,tileName);

        TileItemEditorFactory.saveTile(doorTile,tileName);

        TileItemEditorFactory.generateTileItem(
            tileName: tileName,
            tile: doorTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: door
        );
    }
}

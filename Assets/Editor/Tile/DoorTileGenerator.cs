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
        ItemEditorFactory.createDirectory(tileName);

        StandardTile leftClosedTile = ItemEditorFactory.standardTileCreator(leftClosed,TileColliderType.Sprite);
        ItemEditorFactory.saveTile(leftClosedTile,tileName,"LeftClosed");

        StandardTile leftOpenTile = ItemEditorFactory.standardTileCreator(leftOpen,TileColliderType.Sprite);
        ItemEditorFactory.saveTile(leftOpenTile,tileName,"LeftOpen");

        StandardTile rightClosedTile = ItemEditorFactory.standardTileCreator(rightClosed,TileColliderType.Sprite);
        ItemEditorFactory.saveTile(rightClosedTile,tileName,"RightClosed");

        StandardTile rightOpenTile = ItemEditorFactory.standardTileCreator(rightOpen,TileColliderType.Sprite);
        ItemEditorFactory.saveTile(rightOpenTile,tileName,"RightOpen");

        RestrictedDoorTile doorTile = ScriptableObject.CreateInstance<RestrictedDoorTile>();
        doorTile.left = leftClosedTile;
        doorTile.leftOpen = leftOpenTile;
        doorTile.right = rightClosedTile;
        doorTile.rightOpen = rightOpenTile;
        doorTile.setID(tileName.ToLower().Replace(" ", "_"));

        Door door = ScriptableObject.CreateInstance<Door>();
        ItemEditorFactory.saveTileEntity(door,tileName);

        ItemEditorFactory.saveTile(doorTile,tileName);

        ItemEditorFactory.generateTileItem(
            tileName: tileName,
            tile: doorTile,
            tileType: TileType.Block,
            createFolder: false,
            tileEntity: door
        );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Items;
using Tiles.Fluid;

public class FluidTileGeneratorWindow : EditorWindow {
    private Texture2D texture;
    private TileType tileType;
    private TileColliderType colliderType;
    private string tileName;
    private bool invertedGravity = false;
    private int viscosity = 5;
    private string path;
    [MenuItem("Tools/Item Constructors/Tile/Fluid")]
    public static void ShowWindow()
    {
        FluidTileGeneratorWindow window = (FluidTileGeneratorWindow)EditorWindow.GetWindow(typeof(FluidTileGeneratorWindow));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Sprite", texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(70));
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
  
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Viscosity:", GUILayout.Width(70));
        viscosity = EditorGUILayout.IntField(viscosity);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Inverted Gravity:", GUILayout.Width(70));
        invertedGravity = EditorGUILayout.Toggle(invertedGravity);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Fluid Item"))
        {
            CreateTileItem();
        }
    }

    private void CreateTileItem()
    {
        ItemEditorFactory.CreateDirectory(tileName);
        FluidTileItem fluidTileItem = ScriptableObject.CreateInstance<FluidTileItem>();
        FluidOptions fluidOptions = new FluidOptions();
        fluidOptions.viscosity = viscosity;
        fluidOptions.invertedGravity = invertedGravity;
        
        FluidTile fluidTile = ScriptableObject.CreateInstance<FluidTile>();
        fluidTile.name = "T~" + tileName;
        fluidTile.tiles = EditorFactory.fluidTilesFromSprite(texture,"Assets/EditorCreations/" + tileName, tileName,invertedGravity);
        AssetDatabase.CreateAsset(fluidTile,"Assets/EditorCreations/" + tileName + "/" + fluidTile.name + ".asset");
        
        fluidTileItem.fluidOptions = fluidOptions;
        fluidTileItem.name = tileName;
        fluidTileItem.id = ItemEditorFactory.formatId(tileName);
        fluidTileItem.fluidTile = fluidTile;
        
        
        AssetDatabase.CreateAsset(fluidTileItem,"Assets/EditorCreations/" + tileName + "/" + tileName + ".asset");
        AssetDatabase.Refresh();
    }
}

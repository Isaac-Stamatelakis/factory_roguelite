using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;
using Tiles;

public class StandardHammerTileGenerator : EditorWindow {
    public enum HammerTileType {
        Standard,
        Nature
    }

    private HammerTileType hammerTileType;
    private Sprite sprite;
    private string tileName;
    private string path;
    [MenuItem("Tools/Item Constructors/Tile/Hammer")]
    public static void ShowWindow()
    {
        StandardHammerTileGenerator window = (StandardHammerTileGenerator)EditorWindow.GetWindow(typeof(StandardHammerTileGenerator));
        window.titleContent = new GUIContent("Tile Generator");
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        sprite = EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), true) as Sprite;
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tile Name:", GUILayout.Width(100));
        
        tileName = EditorGUILayout.TextField(tileName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Hammer Type:", GUILayout.Width(100));
        
        hammerTileType = (HammerTileType)EditorGUILayout.EnumPopup(hammerTileType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Tile Item"))
        {
            createTileItem();
        }
    }

    void createTileItem()
    {
        string path = "Assets/EditorCreations/" + tileName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path,true);
        }
        
        AssetDatabase.CreateFolder("Assets/EditorCreations", tileName);
        AssetDatabase.Refresh();


        switch (hammerTileType) {
            case HammerTileType.Standard:
                formStandard(path);
                break;
            case HammerTileType.Nature:
                formNature(path);
                break;

        }
    }

    private void formStandard(string path) {
        StandardTile tile = TileItemEditorFactory.standardTileCreator(sprite,TileColliderType.Tile);
        TileItemEditorFactory.generateTileItem(tileName,tile,TileType.Block);
        TileItemEditorFactory.saveTile(tile,tileName);

        Tile slab = saveSlab(sprite,path,"slab");
        Tile slanted = saveSlant(sprite,path,"slanted");

        HammerTile hammerTile = ScriptableObject.CreateInstance<HammerTile>();
        hammerTile.id = tile.id;
        hammerTile.baseTile = tile;
        hammerTile.cleanSlab = slab;
        hammerTile.cleanSlant = slanted;
        AssetDatabase.CreateAsset(hammerTile,path + "T~"+tileName +"_Hammer" + ".asset");
        AssetDatabase.Refresh();

        TileItemEditorFactory.generateTileItem(tileName,hammerTile,TileType.Block,createFolder:false);
        AssetDatabase.Refresh();
    }

    private Tile saveSlab(Sprite baseSprite, string path, string spriteName) {
        AssetDatabase.CreateFolder("Assets/EditorCreations/" + tileName, spriteName);
        AssetDatabase.Refresh();
        Sprite sprite = TileSpriteShapeFactory.generateSlab(baseSprite.texture,path + "/" + spriteName +"/");
        StandardTile tile = TileItemEditorFactory.standardTileCreator(sprite,TileColliderType.Tile);
        TileItemEditorFactory.saveTile(tile,spriteName,path:path);
        AssetDatabase.Refresh();
        return tile;
    }
    private Tile saveSlant(Sprite baseSprite, string path, string spriteName) {
        AssetDatabase.CreateFolder("Assets/EditorCreations/" + tileName, spriteName);
        AssetDatabase.Refresh();
        Sprite sprite = TileSpriteShapeFactory.generateStandardSlanted(baseSprite.texture,path + "/" + spriteName +"/");
        StandardTile tile = TileItemEditorFactory.standardTileCreator(sprite,TileColliderType.Tile);
        TileItemEditorFactory.saveTile(tile,spriteName,path:path);
        AssetDatabase.Refresh();
        return tile;
    }
    private void formNature(string path) {
        StandardTile tile = TileItemEditorFactory.standardTileCreator(sprite,TileColliderType.Tile);
        TileItemEditorFactory.generateTileItem(tileName,tile,TileType.Block);
        TileItemEditorFactory.saveTile(tile,tileName);

        Tile slab = saveSlab(sprite,path,"slab");
        Tile slanted = saveSlant(sprite,path,"slanted");

        NatureTile natureTile = ScriptableObject.CreateInstance<NatureTile>();
        natureTile.id = tile.id;
        natureTile.baseTile = tile;
        natureTile.cleanSlab = slab;
        natureTile.cleanSlant = slanted;
        
        
        string natureSlantPath = "NatureSlants";
        AssetDatabase.CreateFolder("Assets/EditorCreations/" + tileName, "NatureSlants");
        AssetDatabase.Refresh();
        Sprite[] slantShapes = Resources.LoadAll<Sprite>("Sprites/Shapes/slant_shapes");
        Sprite[] slants = TileSpriteShapeFactory.generateSpritesFromShapeSheet(sprite.texture,path + "/" + natureSlantPath + "/slant_",slantShapes);
        
        natureTile.natureSlants = new Tile[slantShapes.Length];
        for (int i = 0; i < slants.Length; i++) {
            string spriteName = "slants_" + i;
            StandardTile natureSlant = TileItemEditorFactory.standardTileCreator(slants[i],TileColliderType.Sprite);
            natureSlant.id = natureTile.id;
            natureTile.natureSlants[i] = natureSlant;
            AssetDatabase.CreateAsset(natureSlant, path + natureSlantPath + "/" + spriteName + ".asset");
        }

        string natureSlabPath = "NatureSlabs";
        AssetDatabase.CreateFolder("Assets/EditorCreations/" + tileName, natureSlabPath);
        AssetDatabase.Refresh();
        Sprite[] slabShapes = Resources.LoadAll<Sprite>("Sprites/Shapes/slab_shapes");
        Sprite[] slabs = TileSpriteShapeFactory.generateSpritesFromShapeSheet(sprite.texture,path+"/"+ natureSlabPath + "/slab_",slabShapes);
        natureTile.natureSlabs = new Tile[slabs.Length];
        for (int i = 0; i < slabs.Length; i++){
            string spriteName = "slabs_" + i;
            StandardTile natureSlab = TileItemEditorFactory.standardTileCreator(slabs[i],TileColliderType.Sprite);
            natureSlab.id = natureTile.id;
            natureTile.natureSlabs[i] = natureSlab;
            AssetDatabase.CreateAsset(natureSlab, path + natureSlabPath + "/" + spriteName + ".asset");
        }
        AssetDatabase.Refresh();

        AssetDatabase.CreateAsset(natureTile,path + "T~"+ tileName +"_Hammer"+ ".asset");
        AssetDatabase.Refresh();

        TileItemEditorFactory.generateTileItem(tileName,natureTile,TileType.Block,createFolder:false);
        AssetDatabase.Refresh();
    }
}

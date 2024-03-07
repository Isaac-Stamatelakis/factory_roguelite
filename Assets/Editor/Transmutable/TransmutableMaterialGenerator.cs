using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ItemModule.Transmutable;
using ItemModule;

public class TransmutableItemGenerator : EditorWindow {
    private static string GeneratePath = "Assets/Resources/Items/TransmutableItems";
    private static string GenerateFolder = "Items";
    private string FolderPath {get{return GeneratePath +"/" + GenerateFolder;}}
    private Texture2D texture;
    private string tileName;
    [MenuItem("Tools/Item Constructors/Transmutable Materials")]
    public static void ShowWindow()
    {
        TransmutableItemGenerator window = (TransmutableItemGenerator)EditorWindow.GetWindow(typeof(TransmutableItemGenerator));
        window.titleContent = new GUIContent("Material Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generates materials from \nAssets/Resources/Items/Main/TransmutableItems", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUILayout.Label("Deletes and Re-Generates all Materials");
        if (GUILayout.Button("Re-Generate All"))
        {
            regenerate();
        }
        GUILayout.Label("Only Generates New Materials");
        if (GUILayout.Button("Generate New"))
        {
            generateNew();
        }
        
    }

    protected void regenerate() {
        if (Directory.Exists(FolderPath))
        {
            Directory.Delete(FolderPath, true);
            Debug.Log("Folder deleted: " + FolderPath);
        }
        AssetDatabase.Refresh();
        generateNew();
    }
    protected void generateNew() {
        TransmutableItemMaterial[] transmutableItemMaterials = Resources.LoadAll<TransmutableItemMaterial>("");
        TransmutableItemSprites sprites = TransmutableItemSprites.getInstance();
        if (!AssetDatabase.IsValidFolder(FolderPath)) {
            AssetDatabase.CreateFolder(GeneratePath, GenerateFolder);
        }
        foreach (TransmutableItemMaterial transmutableItemMaterial in transmutableItemMaterials) {
            if (AssetDatabase.IsValidFolder(FolderPath + "/" +transmutableItemMaterial.name)){
                Debug.Log("Material " + transmutableItemMaterial.name + " Already Generated");
            } else {
                Debug.Log("Generating Material " + transmutableItemMaterial.name);
                AssetDatabase.CreateFolder(FolderPath,transmutableItemMaterial.name);
                string materialPath = FolderPath + "/" +transmutableItemMaterial.name + "/";
                transmutableItemMaterial.statesToID = new List<KVP<TransmutableItemState, string>>();
                foreach (TransmutableStateOptions itemConstructionData in transmutableItemMaterial.getStates()) {
                    string prefix = TransmutableItemStateExtension.getPrefix(itemConstructionData.state);
                    string suffix = TransmutableItemStateExtension.getSuffix(itemConstructionData.state);
                    string name = "";
                    if (itemConstructionData.prefix.Length == 0) {
                        name += prefix;
                    } else {
                        name = itemConstructionData.prefix;
                    }
                    name += " " + transmutableItemMaterial.name + " ";
                    if (itemConstructionData.suffix.Length == 0) {
                        name += suffix;
                    } else {
                        name += itemConstructionData.suffix;
                    }        
                    transmutableItemMaterial.color.a = 1;
                    Sprite itemSprite = itemConstructionData.sprite;
                    if (itemSprite == null) {
                        itemSprite = sprites.getSprite(itemConstructionData.state);
                    }
                    if (itemSprite == null) {
                        itemSprite = Resources.Load<Sprite>("Sprites/tileobject16by16");
                        Debug.LogError("Attempted to load transmutable item sprite for " + itemConstructionData.state.ToString() + " which does not exist");
                    } else {
                        Texture2D texture2D = itemSprite.texture;
                        Rect rect = new Rect(0, 0, texture2D.width, texture2D.height);
                        Vector2 pivot = new Vector2(0.5f, 0.5f); 
                        Color[] pixels = texture2D.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            pixels[i] *= transmutableItemMaterial.color;
                        }
                        Texture2D newTexture = new Texture2D(texture2D.width, texture2D.height, TextureFormat.RGBA32, texture2D.mipmapCount > 1);
                        newTexture.SetPixels(pixels);
                        newTexture.Apply();
                        
                        string spritePath = materialPath + name.Replace(" ", "") +"Sprite";
                        byte[] pngBytes = newTexture.EncodeToPNG();
                        File.WriteAllBytes(spritePath+".png", pngBytes);
                        AssetDatabase.Refresh();

                        TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath + ".png") as TextureImporter;
                        textureImporter.textureType = TextureImporterType.Sprite;
                        textureImporter.spritePixelsPerUnit = itemSprite.pixelsPerUnit;
                        AssetDatabase.ImportAsset(spritePath + ".png", ImportAssetOptions.ForceUpdate);
                        AssetDatabase.Refresh();

                        itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + ".png");    
                    }
                    string id = (prefix + "_"+ transmutableItemMaterial.id + "_" + suffix).ToLower();
                    TransmutableItemFactory.generateItem(
                        state: itemConstructionData.state,
                        material: transmutableItemMaterial,
                        name: name,
                        sprite: itemSprite,
                        id: id,
                        path: materialPath + name.Replace(" ","") + ".asset"
                    );
                    transmutableItemMaterial.statesToID.Add(new KVP<TransmutableItemState, string>(itemConstructionData.state,id));
                    
                }
                Debug.Log(transmutableItemMaterial.getStates().Count + " Item's created for " + transmutableItemMaterial.name);
                transmutableItemMaterial.test = true;
                EditorUtility.SetDirty(transmutableItemMaterial);
                AssetDatabase.SaveAssets();
            }
            
        }
    }

}




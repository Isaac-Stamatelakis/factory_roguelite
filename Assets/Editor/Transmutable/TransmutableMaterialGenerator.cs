using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TransmutableItemGenerator : EditorWindow {
    private static string GeneratePath = "Assets/Resources/Items/Main/TransmutableItems";
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
            TransmutableMaterialDict dict = new TransmutableMaterialDict(transmutableItemMaterial);
            if (AssetDatabase.IsValidFolder(FolderPath + "/" +transmutableItemMaterial.name)){
                Debug.Log("Material " + transmutableItemMaterial.name + " Already Generated");
            } else {
                Debug.Log("Generating Material " + transmutableItemMaterial.name);
                AssetDatabase.CreateFolder(FolderPath,transmutableItemMaterial.name);
                string materialPath = FolderPath + "/" +transmutableItemMaterial.name + "/";
                foreach (TransmutableStateOptions itemConstructionData in transmutableItemMaterial.getStates()) {
                    string prefix = TransmutableItemStateFactory.getPrefix(itemConstructionData.state);
                    string suffix = TransmutableItemStateFactory.getSuffix(itemConstructionData.state);
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
                    TransmutableItemObject itemObject = ScriptableObject.CreateInstance<TransmutableItemObject>();
                    itemObject.name = name;
                    itemObject.materialDict = dict;
                    itemObject.state = itemConstructionData.state;
                    if (itemConstructionData.sprite == null) {
                        Sprite sprite = sprites.getSprite(itemConstructionData.state);
                        if (sprite == null) {
                            itemObject.sprite = Resources.Load<Sprite>("Sprites/tileobject16by16");
                            Debug.LogError("Attempted to load transmutable item sprite for " + itemConstructionData.state.ToString() + " which does not exist");
                        } else {
                            Texture2D texture2D = sprite.texture;
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
                            textureImporter.spritePixelsPerUnit = sprite.pixelsPerUnit;
                            AssetDatabase.ImportAsset(spritePath + ".png", ImportAssetOptions.ForceUpdate);
                            AssetDatabase.Refresh();

                            Sprite sprite1 = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath + ".png");
                            itemObject.sprite = sprite1;
                        }
                    } else {
                        itemObject.sprite = itemConstructionData.sprite;
                    }
                    itemObject.id = (prefix + "_"+ transmutableItemMaterial.id + "_" + suffix).ToLower();
                    AssetDatabase.CreateAsset(itemObject,materialPath + name.Replace(" ","") + ".asset");
                }
                Debug.Log(transmutableItemMaterial.getStates().Count + " Item's created for " + transmutableItemMaterial.name);
            }
            
        }
    }

}




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Items.Transmutable;
using Items;
using System;
using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TransmutableItemGenerator : EditorWindow
{
    private string GEN_PATH = "Items";
    private string GROUP_NAME = "TransmutableMaterials";
    [MenuItem("Tools/Item Constructors/Transmutable Materials")]
    public static void ShowWindow()
    {
        TransmutableItemGenerator window = (TransmutableItemGenerator)EditorWindow.GetWindow(typeof(TransmutableItemGenerator));
        window.titleContent = new GUIContent("Material Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generates materials from the addressable asset group 'Transmutable Materials'", EditorStyles.boldLabel);
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
        /*
        if (Directory.Exists(FolderPath))
        {
            Directory.Delete(FolderPath, true);
            Debug.Log("Folder deleted: " + FolderPath);
        }
        AssetDatabase.Refresh();
        */
        generateNew();
    }
    protected void generateNew() {
        Debug.Log("Generating Material Items");
        Addressables.LoadAssetsAsync<TransmutableItemMaterial>("transmutable_material",null).Completed += OnAllAssetsLoaded;
        /*
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
                foreach (TransmutableStateOptions itemConstructionData in transmutableItemMaterial.GetStates()) {
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
                    Sprite[] itemSprites = null;
                    transmutableItemMaterial.color.a = 1;
                    if (itemConstructionData.sprites == null || itemConstructionData.sprites.Length == 0) {
                        Sprite itemSprite = sprites.getSprite(itemConstructionData.state);
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
                            itemSprites = new Sprite[]{itemSprite};
                        }
                    } else {
                        itemSprites = itemConstructionData.sprites;
                    }

                    string id = TransmutableItemUtils.GetStateId(transmutableItemMaterial, itemConstructionData); 
                    TransmutableItemFactory.generateItem(
                        state: itemConstructionData.state,
                        material: transmutableItemMaterial,
                        name: name,
                        sprites: itemSprites,
                        id: id,
                        path: materialPath + name.Replace(" ","") + ".asset"
                    );
                }
                Debug.Log(transmutableItemMaterial.GetStates().Count + " Item's created for " + transmutableItemMaterial.name);
                EditorUtility.SetDirty(transmutableItemMaterial);
                AssetDatabase.SaveAssets();
            }
        }
        */
    }
    
    private void OnAllAssetsLoaded(AsyncOperationHandle<IList<TransmutableItemMaterial>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Materials loaded from addressable group");
            foreach (var asset in handle.Result)
            {
                GenerateMaterialItems(asset);
            }
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Failed to load assets.");
        }
    }

    private void GenerateMaterialItems(TransmutableItemMaterial material)
    {
        string assetPath = AssetDatabase.GetAssetPath(material);
        
        string materialFolder = Path.GetDirectoryName(assetPath);
        string transmutableItemFolder = Path.GetDirectoryName(materialFolder);
        Assert.AreEqual("Assets\\Objects\\Items\\TransmutableItems", transmutableItemFolder);
        TransmutableMaterialOptions options = material.MaterialOptions;
        if (options == null)
        {
            return;
        }
        string instancePath = Path.Combine(transmutableItemFolder, GEN_PATH);
        if (!Directory.Exists(instancePath))
        {
            AssetDatabase.CreateFolder(transmutableItemFolder, GEN_PATH);
        }
        string materialItemsPath = Path.Combine(instancePath, material.name);
        if (!Directory.Exists(materialItemsPath))
        {
            AssetDatabase.CreateFolder(instancePath, material.name);
        }
        foreach (TransmutableStateOptions stateOptions in options.States)
        {
            string id = TransmutableItemUtils.GetStateId(material, stateOptions);
            string itemName = TransmutableItemUtils.GetStateName(material,stateOptions);
            string savePath = Path.Combine(materialItemsPath, itemName + ".asset");
            bool exists = AssetDatabase.LoadAssetAtPath<ScriptableObject>(savePath) != null;
            if (exists)
            {
                continue;
            }
            TransmutableItemObject transmutableItemObject = CreateInstance<TransmutableItemObject>();
            transmutableItemObject.name = itemName;
            transmutableItemObject.id = id;
            transmutableItemObject.setMaterial(material);
            transmutableItemObject.setState(stateOptions.state);
            AssetDatabase.CreateAsset(transmutableItemObject,  savePath);
        }
    }

}




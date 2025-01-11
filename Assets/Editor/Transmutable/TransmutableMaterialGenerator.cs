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
    [MenuItem("ToolCollection/Item Constructors/Transmutable Materials")]
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




using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Items.Transmutable;
using Items;
using System;
using Item.Slot;
using NUnit.Framework;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TransmutableItemGenerator : EditorWindow
{
    private const string GEN_PATH = "Items";
    [MenuItem("Tools/Item Constructors/Transmutable Materials")]
    public static void ShowWindow()
    {
        TransmutableItemGenerator window = (TransmutableItemGenerator)EditorWindow.GetWindow(typeof(TransmutableItemGenerator));
        window.titleContent = new GUIContent("Material Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Generates materials with from addressables with label 'transmutable_material' 'Transmutable Materials'", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUI.enabled = false;
        GUILayout.TextArea("Generates new materials, adds new states for existing materials and deletes states that no longer exist from existing materials.");
        GUI.enabled = true;
        if (GUILayout.Button("Update Material Items"))
        {
            UpdateExisting();
        }
        
    }
    
    protected void UpdateExisting()
    {
        Debug.Log("Loading assets from addrssables");
        Addressables.LoadAssetsAsync<TransmutableItemMaterial>("transmutable_material",null).Completed += OnLoadUpdate;
    }
    
    private void OnLoadUpdate(AsyncOperationHandle<IList<TransmutableItemMaterial>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"Loaded {handle.Result.Count} materials from addressable");
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
            Debug.Log($"Created folder for {material.name}");
            AssetDatabase.CreateFolder(instancePath, material.name);
        }
        string[] guids = AssetDatabase.FindAssets("", new[] { materialItemsPath });
        
       
        var stateItemDict = new Dictionary<TransmutableItemState, TransmutableItemObject>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TransmutableItemObject transmutableItemObject = AssetDatabase.LoadAssetAtPath<TransmutableItemObject>(path);
            if (ReferenceEquals(transmutableItemObject, null)) continue;
            if (stateItemDict.ContainsKey(transmutableItemObject.getState()))
            {
                Debug.LogWarning($"Material {material.name} has duplicate items for state {transmutableItemObject.getItemState()}");
            }
            stateItemDict[transmutableItemObject.getState()] = transmutableItemObject;
        }
        

        CreateNew(material, materialItemsPath, stateItemDict, out var materialStates);
        RemovedUnusedStates(stateItemDict, materialStates);
        ValidateItems(material, stateItemDict);

    }

    private void CreateNew(TransmutableItemMaterial material, string materialItemsPath, Dictionary<TransmutableItemState, TransmutableItemObject> stateItemDict, out HashSet<TransmutableItemState> materialStates)
    {
        materialStates = new HashSet<TransmutableItemState>();
        foreach (TransmutableStateOptions stateOptions in material.MaterialOptions.States)
        {
            materialStates.Add(stateOptions.state);
            if (stateItemDict.ContainsKey(stateOptions.state)) continue;
            
            string id = TransmutableItemUtils.GetStateId(material, stateOptions);
            string itemName = TransmutableItemUtils.GetStateName(material,stateOptions);
            string savePath = GetStateAssetPath(materialItemsPath, itemName);
            
            TransmutableItemObject transmutableItemObject = CreateInstance<TransmutableItemObject>();
            transmutableItemObject.name = itemName;
            transmutableItemObject.id = id;
            transmutableItemObject.setMaterial(material);
            transmutableItemObject.setState(stateOptions.state);
            AssetDatabase.CreateAsset(transmutableItemObject,  savePath);
            Debug.Log($"Created '{itemName}'");
            stateItemDict[stateOptions.state] = transmutableItemObject;
        }
    }

    private void RemovedUnusedStates(Dictionary<TransmutableItemState, TransmutableItemObject> stateItemDict, HashSet<TransmutableItemState> materialStates)
    {
        foreach (var (state, transmutableItemObject) in stateItemDict)
        {
            if (materialStates.Contains(state)) continue;
            Debug.Log($"Removed '{transmutableItemObject.name}'");
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(transmutableItemObject));
        }
    }

    private void ValidateItems(TransmutableItemMaterial material, Dictionary<TransmutableItemState, TransmutableItemObject> stateItemDict)
    {
        foreach (var (state, transmutableItemObject) in stateItemDict)
        {
            transmutableItemObject.gameStage = material.gameStageObject;
            AssetDatabase.SaveAssetIfDirty(transmutableItemObject);
        }
    }

    private string GetStateAssetPath(string materialItemsPath, string itemName)
    {
        return Path.Combine(materialItemsPath, itemName + ".asset");
    }

}




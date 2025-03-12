using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Items.Transmutable;
using Items;
using System;
using Item.GameStage;
using Item.Slot;
using NUnit.Framework;
using Tiles;
using Tiles.Options.Overlay;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using World.Cave.Collections;

public class TransmutableItemGenerator : EditorWindow
{
    private const string OUTLINE_WRAPPER_PATH = "Assets/Objects/Items/TransmutableItems/OreSource/OutlineWrapper.asset";
    private const string STONE_COLLECTION_PATH = "Assets/Objects/Items/TransmutableItems/OreSource/StoneCollection.asset";
    private const string GAMESTAGE_PATH = "Assets/Objects/Items/TransmutableItems/OreSource/ORE.asset";
    private const string GEN_PATH = "Items";
    private const string ORE_PATH = "Ores";
    private const string ORE_OVERLAY_NAME = "_Overlay";
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
        GUI.enabled = false;
        GUILayout.TextArea("For all existing materials, generates ores for all existing stones and removes ores derived from stones which now longer exist.");
        GUI.enabled = true;
        if (GUILayout.Button("Update Ore Items"))
        {
            UpdateOres();
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

    private string GetMaterialItemPath(TransmutableItemMaterial material)
    {
        string assetPath = AssetDatabase.GetAssetPath(material);
        
        string materialFolder = Path.GetDirectoryName(assetPath);
        
        string transmutableItemFolder = Path.GetDirectoryName(materialFolder);
        Assert.AreEqual("Assets\\Objects\\Items\\TransmutableItems", transmutableItemFolder);
        return transmutableItemFolder;
    }
    private void GenerateMaterialItems(TransmutableItemMaterial material)
    {
        string transmutableItemFolder = GetMaterialItemPath(material);
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
    

    private string GetStateAssetPath(string materialItemsPath, string itemName)
    {
        return Path.Combine(materialItemsPath, itemName + ".asset");
    }

    private void UpdateOres()
    {
        Addressables.LoadAssetsAsync<TransmutableItemMaterial>("transmutable_material",null).Completed += OnOreLoadUpdate;
    }
    
    private void OnOreLoadUpdate(AsyncOperationHandle<IList<TransmutableItemMaterial>> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            TileWrapperObject outlineWrapper = AssetDatabase.LoadAssetAtPath<TileWrapperObject>(OUTLINE_WRAPPER_PATH);
            StoneTileCollection stoneTileCollection = AssetDatabase.LoadAssetAtPath<StoneTileCollection>(STONE_COLLECTION_PATH);
            GameStageObject oreGameStage = AssetDatabase.LoadAssetAtPath<GameStageObject>(GAMESTAGE_PATH);
            Debug.Log($"Loaded {handle.Result.Count} materials from addressable");
            foreach (var asset in handle.Result)
            {
                Debug.Log(asset.name);
                GenerateOreItems(asset, outlineWrapper, stoneTileCollection, oreGameStage);
                break;
            }
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("Failed to load assets.");
        }
    }

    private void GenerateOreItems(TransmutableItemMaterial material, TileWrapperObject outlineWrapper, StoneTileCollection stoneTileCollection, GameStageObject oreGameStage)
    {
        string transmutableItemFolder = GetMaterialItemPath(material);
        string instancePath = Path.Combine(transmutableItemFolder, GEN_PATH);
        if (!Directory.Exists(instancePath)) return;
        string materialItemsPath = Path.Combine(instancePath, material.name);
        if (!Directory.Exists(materialItemsPath)) return;

        TransmutableItemObject oreItem = GetOreItem(materialItemsPath);
        if (!oreItem) return;
        
        string oreFolderPath = Path.Combine(materialItemsPath, ORE_PATH);
        if (!Directory.Exists(oreFolderPath))
        {
            Debug.Log($"Created ore folder for {material.name}");
            AssetDatabase.CreateFolder(materialItemsPath, ORE_PATH);
        }


        string overlayPath = Path.Combine(oreFolderPath, ORE_OVERLAY_NAME + ".asset");
        TransmutableTileOverlay tileOverlay = AssetDatabase.LoadAssetAtPath<TransmutableTileOverlay>(overlayPath);
        if (!tileOverlay)
        {
            tileOverlay = CreateInstance<TransmutableTileOverlay>();
            tileOverlay.ItemMaterial = material;
            tileOverlay.name = ORE_OVERLAY_NAME;
            tileOverlay.OverlayWrapper = outlineWrapper;
            string savePath = overlayPath + ".asset";
            Debug.Log(savePath);
            AssetDatabase.CreateAsset(tileOverlay, savePath);
            AssetDatabase.Refresh();
            tileOverlay = AssetDatabase.LoadAssetAtPath<TransmutableTileOverlay>(overlayPath);
        }
        
        string[] guids = AssetDatabase.FindAssets("", new[] { oreFolderPath });
        
        HashSet<string> existingOres = new HashSet<string>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TileItem tileItem = AssetDatabase.LoadAssetAtPath<TileItem>(path);
            if (ReferenceEquals(tileItem, null)) continue;
            string id = tileItem.id;
            string stoneId = id.Replace("_ore_", "").Replace(material.name.ToLower(), "");
            existingOres.Add(stoneId);
        }
        
        HashSet<string> stoneIds = new HashSet<string>();

        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.FindGroup("Items");
        
        foreach (TileItem tileItem in stoneTileCollection.Tiles)
        {
            if (!tileItem) continue;
            string id = tileItem.id;
            if (id == null) continue;
            stoneIds.Add(id);
            if (existingOres.Contains(id)) continue;
            TileItem oreTile = CreateInstance<TileItem>();
            oreTile.tile = tileItem.tile;
            oreTile.outline = tileItem.outline;
            oreTile.gameStage = oreGameStage;
            oreTile.tileOptions = new TileOptions
            {
                hardness = tileItem.tileOptions.hardness,
                rotatable = tileItem.tileOptions.rotatable,
                movementModifier = tileItem.tileOptions.movementModifier,
                requiredToolTier = tileItem.tileOptions.requiredToolTier,
                ParticleGradient = tileItem.tileOptions.ParticleGradient,
                Overlay = tileOverlay
            };
            oreTile.id = TransmutableItemUtils.GetOreId(id, material);
            oreTile.name = $"{tileItem.name} {material.name} Ore";
            DropOption dropOption = new DropOption
            {
                itemObject = oreItem,
                weight = 1,
                lowerAmount = 1,
                upperAmount = 1
            };
            oreTile.tileOptions.dropOptions = new List<DropOption>
            {
                dropOption
            };
            string savePath = Path.Combine(oreFolderPath, oreTile.name + ".asset");
            AssetDatabase.CreateAsset(oreTile, savePath);
            AssetDatabase.SaveAssets();
            
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(savePath), group);
            entry.labels.Add("item");
            entry.SetAddress(oreTile.name);
            
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TileItem tileItem = AssetDatabase.LoadAssetAtPath<TileItem>(path);
            if (ReferenceEquals(tileItem, null)) continue;
            string id = tileItem.id;
            string stoneId = id.Replace("_ore_", "").Replace(material.name.ToLower(), "");
            if (stoneIds.Contains(stoneId)) continue;
            AssetDatabase.DeleteAsset(path);
        }
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }

    private TransmutableItemObject GetOreItem(string materialItemsPath)
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { materialItemsPath });
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TransmutableItemObject transmutableItemObject = AssetDatabase.LoadAssetAtPath<TransmutableItemObject>(path);
            if (ReferenceEquals(transmutableItemObject, null)) continue;
            if (transmutableItemObject.getState() == TransmutableItemState.Ore) return transmutableItemObject;
        }

        return null;
    }
}




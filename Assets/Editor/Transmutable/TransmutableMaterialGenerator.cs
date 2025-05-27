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
using Item.Transmutation;
using NUnit.Framework;
using Tiles;
using Tiles.Options.Overlay;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using World.Cave.Collections;
using Object = UnityEngine.Object;

public class TransmutableItemGenerator : EditorWindow
{
    private const string MATERIAL_PATH = "Assets/Objects/TransmutableItems/Materials";
    private const string OUTLINE_WRAPPER_PATH = "Assets/Objects/TransmutableItems/OreSource/OutlineWrapper.asset";
    private const string SHADER_OUTLINE_WRAPPER_PATH = "Assets/Objects/TransmutableItems/OreSource/ShaderOutlineWrapper.asset";
    private const string STONE_COLLECTION_PATH = "Assets/Objects/TransmutableItems/OreSource/StoneCollection.asset";
    private const string GAMESTAGE_PATH = "Assets/Objects/TransmutableItems/OreSource/ORE.asset";
    public const string GEN_FOLDER = "Items";
    private const string ORE_PATH = "Ores";
    private const string ORE_OVERLAY_NAME = "_Overlay";
    private const string MISC_PATH = "Misc";
    [MenuItem("Tools/Item Constructors/Transmutable Materials")]
    public static void ShowWindow()
    {
        TransmutableItemGenerator window = (TransmutableItemGenerator)EditorWindow.GetWindow(typeof(TransmutableItemGenerator));
        window.titleContent = new GUIContent("Material Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label($"Generates items for materials inside {MATERIAL_PATH}", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        GUI.enabled = false;
        GUILayout.TextArea("Generates items for new materials, adds new states for existing materials and deletes states that no longer exist from existing materials.");
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
            UpdateOres(false);
        }
        
        GUI.enabled = false;
        GUILayout.TextArea("For all existing materials, deletes and re-generates ores.");
        GUI.enabled = true;
        if (GUILayout.Button("Re-Build Ore Items"))
        {
            UpdateOres(true);
        }
        
    }
    
    protected void UpdateExisting()
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { MATERIAL_PATH });
        Debug.Log("Updating Transmutable Materials");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset is not TransmutableItemMaterial transmutableItemMaterial) continue;
            GenerateMaterialItems(transmutableItemMaterial);
        }
        AssetDatabase.Refresh();
        Debug.Log("Complete");
    }
    
    private static string GetMaterialItemPath(TransmutableItemMaterial material)
    {
        string assetPath = AssetDatabase.GetAssetPath(material);
        
        string materialFolder = Path.GetDirectoryName(assetPath);
        
        string transmutableItemFolder = Path.GetDirectoryName(materialFolder);
        Assert.AreEqual("Assets\\Objects\\TransmutableItems", transmutableItemFolder);
        return transmutableItemFolder;
    }
    private void GenerateMaterialItems(TransmutableItemMaterial material)
    {
        string transmutableItemFolder = GetMaterialItemPath(material);
        TransmutableMaterialOptions options = material.MaterialOptions;
        if (!options)
        {
            Debug.LogWarning($"Material '{material}' options not set.");
            return;
        }
        string instancePath = Path.Combine(transmutableItemFolder, GEN_FOLDER);
        if (!Directory.Exists(instancePath))
        {
            AssetDatabase.CreateFolder(transmutableItemFolder, GEN_FOLDER);
        }
        string materialItemsPath = Path.Combine(instancePath, material.name);
        if (!Directory.Exists(materialItemsPath))
        {
            Debug.Log($"Created folder for {material.name}");
            AssetDatabase.CreateFolder(instancePath, material.name);
        }
        
        string miscPath = Path.Combine(materialItemsPath, MISC_PATH);
        if (!Directory.Exists(miscPath))
        {
            Debug.Log($"Created misc folder for {material.name}");
            AssetDatabase.CreateFolder(materialItemsPath, MISC_PATH);
        }
        string[] guids = AssetDatabase.FindAssets("", new[] { materialItemsPath });
        
        var stateItemDict = new Dictionary<TransmutableItemState, ITransmutableItem>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemObject transmutableItemObject = AssetDatabase.LoadAssetAtPath<ItemObject>(path);
            if (transmutableItemObject is not ITransmutableItem transmutableItem) continue;
            if (stateItemDict.ContainsKey(transmutableItem.getState()))
            {
                Debug.LogWarning($"Material {material.name} has duplicate items for state {transmutableItem.getState()}");
            }
            stateItemDict[transmutableItem.getState()] = transmutableItem;
        }
        
        CreateNew(material, materialItemsPath, stateItemDict, out var materialStates);
        RemovedUnusedStates(stateItemDict, materialStates);

    }

    private void CreateNew(TransmutableItemMaterial material, string materialItemsPath, Dictionary<TransmutableItemState, ITransmutableItem> stateItemDict, out HashSet<TransmutableItemState> materialStates)
    {
        materialStates = new HashSet<TransmutableItemState>();
        List<AssetLabel> labels = new List<AssetLabel> { AssetLabel.Item };
        
        foreach (TransmutableStateOptions stateOptions in material.MaterialOptions.States)
        {
            TransmutableItemState state = (TransmutableItemState)stateOptions.state;
            materialStates.Add(state);
            if (stateItemDict.ContainsKey(state)) continue;
            
            string id = TransmutableItemUtils.GetStateId(material, state);
            string itemName = TransmutableItemUtils.GetStateName(material,state);
            string savePath = GetStateAssetPath(materialItemsPath, itemName);
            
            TransmutableItemObject transmutableItemObject = CreateInstance<TransmutableItemObject>();
            transmutableItemObject.name = itemName;
            transmutableItemObject.id = id;
            transmutableItemObject.setMaterial(material);
            transmutableItemObject.setState(state);
            AssetDatabase.CreateAsset(transmutableItemObject,  savePath);
            
            Debug.Log($"Created '{itemName}'");
            string guid = AssetDatabase.AssetPathToGUID(savePath);
            EditorHelper.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            stateItemDict[state] = transmutableItemObject;
        }
        
        foreach (TransmutableTileStateOptions tileStateOptions in material.MaterialOptions.TileStates)
        {
            TransmutableItemState state = (TransmutableItemState)tileStateOptions.state;
            materialStates.Add(state);
            if (stateItemDict.ContainsKey(state)) continue;
            
            string id = TransmutableItemUtils.GetStateId(material, state);
            string itemName = TransmutableItemUtils.GetStateName(material,state);
            string savePath = GetStateAssetPath(materialItemsPath, itemName);
            
            TransmutableTileItem transmutableTileItem = CreateInstance<TransmutableTileItem>();
            transmutableTileItem.name = itemName;
            transmutableTileItem.id = id;
            transmutableTileItem.tileType = TileType.Block;
            transmutableTileItem.setMaterial(material);
            transmutableTileItem.setState(state);
            transmutableTileItem.tile = tileStateOptions.tile;
            TileOptions tileOptions = new TileOptions();
            tileOptions.TransmutableColorOverride = material;

            if (material.WorldShaderMaterial)
            {
                string miscPath = Path.Combine(materialItemsPath, MISC_PATH);
                TransmutableTileOverlay transmutableTileOverlay = ScriptableObject.CreateInstance<TransmutableTileOverlay>();
                transmutableTileOverlay.ItemMaterial = material;
                transmutableTileOverlay.name = itemName + "_Overlay";
                
                TileWrapperObject tileWrapperObject = ScriptableObject.CreateInstance<TileWrapperObject>();
                transmutableTileOverlay.OverlayWrapper = tileWrapperObject;
                tileWrapperObject.TileBase = tileStateOptions.tile;
                tileWrapperObject.name = itemName +"_Overlay_Tile_Wrapper";
                tileOptions.Overlay = transmutableTileOverlay;
                
                AssetDatabase.CreateAsset(transmutableTileOverlay,  Path.Combine(miscPath,transmutableTileOverlay.name + ".asset"));
                AssetDatabase.CreateAsset(tileWrapperObject,   Path.Combine(miscPath,tileWrapperObject.name + ".asset"));
            }
            int tierInt = (int)(material.gameStageObject?.Tier ?? TileEntity.Tier.Basic);
            tileOptions.hardness = 8 * (tierInt + 1);
            transmutableTileItem.tileOptions = tileOptions;
            AssetDatabase.CreateAsset(transmutableTileItem,  savePath);
            
            Debug.Log($"Created '{itemName}'");
            string guid = AssetDatabase.AssetPathToGUID(savePath);
            EditorHelper.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            stateItemDict[state] = transmutableTileItem;
        }
        return;

    }

    private void RemovedUnusedStates(Dictionary<TransmutableItemState, ITransmutableItem> stateItemDict, HashSet<TransmutableItemState> materialStates)
    {
        foreach (var (state, transmutableItemObject) in stateItemDict)
        {
            if (materialStates.Contains(state)) continue;
            ItemObject itemObject = (ItemObject)transmutableItemObject;
            Debug.Log($"Removed '{itemObject.name}'");
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(itemObject));
        }
    }
    

    private static string GetStateAssetPath(string materialItemsPath, string itemName)
    {
        return Path.Combine(materialItemsPath, itemName + ".asset");
    }
    

    private void UpdateOres(bool reset)
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { MATERIAL_PATH });
        TileWrapperObject outlineWrapper = AssetDatabase.LoadAssetAtPath<TileWrapperObject>(OUTLINE_WRAPPER_PATH);
        TileWrapperObject shaderOutlineWrapper = AssetDatabase.LoadAssetAtPath<TileWrapperObject>(SHADER_OUTLINE_WRAPPER_PATH);
        StoneTileCollection stoneTileCollection = AssetDatabase.LoadAssetAtPath<StoneTileCollection>(STONE_COLLECTION_PATH);
        GameStageObject oreGameStage = AssetDatabase.LoadAssetAtPath<GameStageObject>(GAMESTAGE_PATH);
        string debugText = reset ? "Deleting and Rebuilding all Ores" : "Updating Ores";
        Debug.Log(debugText);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset is not TransmutableItemMaterial transmutableItemMaterial) continue;
            GenerateOreItems(transmutableItemMaterial, outlineWrapper, shaderOutlineWrapper,stoneTileCollection, oreGameStage,reset);
        }
        AssetDatabase.Refresh();
        Debug.Log("Complete");
    }

    private void GenerateOreItems(TransmutableItemMaterial material, TileWrapperObject outlineWrapper, TileWrapperObject shaderOutlineWrapper, StoneTileCollection stoneTileCollection, GameStageObject oreGameStage, bool reset)
    {
        string transmutableItemFolder = GetMaterialItemPath(material);
        string instancePath = Path.Combine(transmutableItemFolder, GEN_FOLDER);
        if (!Directory.Exists(instancePath)) return;
        string materialItemsPath = Path.Combine(instancePath, material.name);
        if (!Directory.Exists(materialItemsPath)) return;

        TransmutableItemObject oreItem = GetOreItem(materialItemsPath);
        if (!oreItem) return;
        
        string oreFolderPath = Path.Combine(materialItemsPath, ORE_PATH);
        if (reset && Directory.Exists(oreFolderPath))
        {
            Directory.Delete(oreFolderPath, true);
            AssetDatabase.Refresh();
        }
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
            tileOverlay.OverlayWrapper = material.WorldShaderMaterial ? shaderOutlineWrapper : outlineWrapper;
            string savePath = overlayPath + ".asset";
            AssetDatabase.CreateAsset(tileOverlay, savePath);
            AssetDatabase.SaveAssets();
            tileOverlay = AssetDatabase.LoadAssetAtPath<TransmutableTileOverlay>(savePath);
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
        
        List<AssetLabel> labels = new List<AssetLabel> { AssetLabel.Item };
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
            string guid = AssetDatabase.AssetPathToGUID(savePath);
            
            EditorHelper.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            
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




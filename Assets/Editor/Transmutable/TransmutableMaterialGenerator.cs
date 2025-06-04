using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Items.Transmutable;
using Items;
using System;
using System.Diagnostics;
using Item.GameStage;
using Item.Slot;
using Item.Transmutation;
using Item.Transmutation.Items;
using NUnit.Framework;
using Tiles;
using Tiles.CustomTiles;
using Tiles.Options.Overlay;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using World.Cave.Collections;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public class TransmutableItemGenerator : EditorWindow
{
    private const string MATERIAL_PATH = "Assets/Objects/TransmutableItems/Materials";
    private const string TRANSMUTABLE_MATERIAL_PATH = "Assets\\Objects\\TransmutableItems";
    private const string OUTLINE_WRAPPER_PATH = "Assets/Objects/TransmutableItems/OreSource/OutlineWrapper.asset";
    private const string SHADER_OUTLINE_WRAPPER_PATH = "Assets/Objects/TransmutableItems/OreSource/ShaderOutlineWrapper.asset";
    private const string STONE_COLLECTION_PATH = "Assets/Objects/TransmutableItems/OreSource/StoneCollection.asset";
    private const string GAMESTAGE_PATH = "Assets/Objects/TransmutableItems/OreSource/ORE.asset";
    public const string GEN_FOLDER = "Items";
    private const string ORE_PATH = "Ores";
    private const string ORE_OVERLAY_NAME = "_Overlay";
    private const string MISC_PATH = "Misc";

    private OutlineValues outlineValues;
    private TransmutableItemState resetState = TransmutableItemState.Block;
    [MenuItem("Tools/Item Constructors/Transmutable Materials")]
    public static void ShowWindow()
    {
        TransmutableItemGenerator window = (TransmutableItemGenerator)EditorWindow.GetWindow(typeof(TransmutableItemGenerator));
        window.titleContent = new GUIContent("Material Item Generator");
    }

    public void OnEnable()
    {
         outlineValues = new OutlineValues();
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
        
        GUI.enabled = false;
        GUILayout.TextArea("For all existing materials, deletes and re-generates items of ResetState.");
        GUI.enabled = true;
        resetState = (TransmutableItemState)EditorGUILayout.EnumPopup("State to Rebuild", resetState);
        if (GUILayout.Button("Re-Build State"))
        {
            ResetState(resetState);
        }
    }
    
    protected void UpdateExisting()
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { MATERIAL_PATH });
        Debug.Log("Updating Transmutable Materials");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset is not TransmutableItemMaterial transmutableItemMaterial) continue;
            GenerateMaterialItems(transmutableItemMaterial);
        }
        AssetDatabase.Refresh();
        Debug.Log($"Updated Materials in {stopwatch.Elapsed.TotalSeconds:F2}s");
    }

    protected void ResetState(TransmutableItemState state)
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { MATERIAL_PATH });
        Debug.Log("Updating Transmutable Materials");
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset is not TransmutableItemMaterial transmutableItemMaterial) continue;
            ResetMaterial(transmutableItemMaterial);

        }
        AssetDatabase.Refresh();
        Debug.Log($"Re-built '{state}' in {stopwatch.Elapsed.TotalSeconds:F2}s");

        return;

        void ResetMaterial(TransmutableItemMaterial material)
        {
            string materialItemsPath = TryCreateMaterialFolder(material);
            var transmutableItemDict = new Dictionary<TransmutableItemState, ITransmutableItem>();
            var transmutableItemStates = Enum.GetValues(typeof(TransmutableItemState));
            
            string itemName = TransmutableItemUtils.GetStateName(material,state);
            string savePath = GetStateAssetPath(materialItemsPath, itemName);
            
            foreach (TransmutableItemState transmutableItemState in transmutableItemStates)
            {
                transmutableItemDict[transmutableItemState] = null;
            }
            
            ItemObject itemObject = AssetDatabase.LoadAssetAtPath<ItemObject>(savePath);
            if (itemObject is ITransmutableItem transmutableItem)
            {
                transmutableItemDict[state] = transmutableItem;
                HashSet<TransmutableItemState> statesToRemove = new HashSet<TransmutableItemState>
                {
                    state
                };
                RemoveStates(transmutableItemDict,statesToRemove);
            }
            transmutableItemDict.Remove(state);
            CreateNew(material, materialItemsPath, transmutableItemDict, out _);
        }
    }
    private void GenerateMaterialItems(TransmutableItemMaterial material)
    {
        TransmutableMaterialOptions options = material.MaterialOptions;
        if (!options)
        {
            Debug.LogWarning($"Material '{material}' options not set.");
            return;
        }
        string materialItemsPath = TryCreateMaterialFolder(material);
        
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
        RemoveStates(stateItemDict, materialStates);

    }

    string TryCreateMaterialFolder(TransmutableItemMaterial material)
    {
        string instancePath = Path.Combine(TRANSMUTABLE_MATERIAL_PATH, GEN_FOLDER);
        if (!Directory.Exists(instancePath))
        {
            AssetDatabase.CreateFolder(TRANSMUTABLE_MATERIAL_PATH, GEN_FOLDER);
        }
        string materialItemsPath = Path.Combine(instancePath, material.name);
        if (!Directory.Exists(materialItemsPath))
        {
            Debug.Log($"Created folder for {material.name}");
            AssetDatabase.CreateFolder(instancePath, material.name);
        }
        return materialItemsPath;
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
            EditorUtils.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
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
            TileType tileType = tileStateOptions.tile is BackgroundRuleTile ? TileType.Background : TileType.Block;
            transmutableTileItem.tileType = tileType;
            transmutableTileItem.setMaterial(material);
            transmutableTileItem.setState(state);
            transmutableTileItem.gameStage = material.gameStageObject;
            transmutableTileItem.tile = tileStateOptions.tile;
            transmutableTileItem.outline = outlineValues.FromTile(transmutableTileItem.tile);
            
            TileOptions tileOptions = new TileOptions();
            tileOptions.TransmutableColorOverride = material;
            tileOptions.rotatable = tileType==TileType.Block;
            
            int tierInt = (int)(material.gameStageObject?.Tier ?? TileEntity.Tier.Basic);
            tileOptions.hardness = 8 * (tierInt + 1);
            transmutableTileItem.tileOptions = tileOptions;
            AssetDatabase.CreateAsset(transmutableTileItem,  savePath);
            
            Debug.Log($"Created '{itemName}'");
            string guid = AssetDatabase.AssetPathToGUID(savePath);
            EditorUtils.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            stateItemDict[state] = transmutableTileItem;
        }
        
        foreach (TransmutableFluidTileOptions fluidStateOptions in material.MaterialOptions.FluidStates)
        {
            TransmutableItemState state = (TransmutableItemState)fluidStateOptions.state;
            materialStates.Add(state);
            if (stateItemDict.ContainsKey(state)) continue;
            
            string id = TransmutableItemUtils.GetStateId(material, state);
            string itemName = TransmutableItemUtils.GetStateName(material,state);
            string savePath = GetStateAssetPath(materialItemsPath, itemName);
            
            TransmutableFluidTileItemObject fluidTileItem = CreateInstance<TransmutableFluidTileItemObject>();
            fluidTileItem.name = itemName;
            fluidTileItem.id = id;
            fluidTileItem.fluidTile = material.HasShaders ? fluidStateOptions.unpackedTile : fluidStateOptions.packedTile;
            fluidTileItem.setMaterial(material);
            fluidTileItem.setState(state);
            fluidTileItem.GameStageObject = material.gameStageObject;
            
            FluidOptions fluidOptions = new FluidOptions();
            fluidOptions.MaterialColorOverride = material;
            fluidOptions.viscosity = fluidStateOptions.viscosity;
            fluidOptions.Opacity = fluidStateOptions.opacity;
            fluidOptions.DamagePerSecond = fluidStateOptions.damage;
            fluidOptions.Lit =  fluidStateOptions.lit;
            fluidOptions.invertedGravity = fluidStateOptions.state == TransmutableFluidItemState.Gas;
            fluidTileItem.fluidOptions = fluidOptions;
            
            AssetDatabase.CreateAsset(fluidTileItem,  savePath);
            
            Debug.Log($"Created '{itemName}'");
            string guid = AssetDatabase.AssetPathToGUID(savePath);
            EditorUtils.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            stateItemDict[state] = fluidTileItem;
        }
        return;

    }

    private void TryCreateMiscFolder(string materialInstancePath, TransmutableItemMaterial material)
    {
        string miscPath = Path.Combine(materialInstancePath, MISC_PATH);
        if (!Directory.Exists(miscPath))
        {
            Debug.Log($"Created misc folder for {material.name}");
            AssetDatabase.CreateFolder(materialInstancePath, MISC_PATH);
        }
    }
    private void RemoveStates(Dictionary<TransmutableItemState, ITransmutableItem> stateItemDict, HashSet<TransmutableItemState> materialStates)
    {
        foreach (var (state, transmutableItemObject) in stateItemDict)
        {
            if (materialStates.Contains(state) || transmutableItemObject == null) continue;
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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
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
        Debug.Log($"Update Ores in {stopwatch.Elapsed.TotalSeconds:F2}s");
    }

    private void GenerateOreItems(TransmutableItemMaterial material, TileWrapperObject outlineWrapper, TileWrapperObject shaderOutlineWrapper, StoneTileCollection stoneTileCollection, GameStageObject oreGameStage, bool reset)
    {
        TransmutationShaderPair shaderPair = material.GetShaderPair();
        string instancePath = Path.Combine(TRANSMUTABLE_MATERIAL_PATH, GEN_FOLDER);
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
        TransmutableTileOverlayData tileOverlayData = AssetDatabase.LoadAssetAtPath<TransmutableTileOverlayData>(overlayPath);
        if (!tileOverlayData)
        {
            tileOverlayData = CreateInstance<TransmutableTileOverlayData>();
            tileOverlayData.ItemMaterial = material;
            tileOverlayData.name = ORE_OVERLAY_NAME;
            tileOverlayData.OverlayWrapper = shaderPair?.WorldMaterial ? shaderOutlineWrapper : outlineWrapper;
            string savePath = overlayPath + ".asset";
            AssetDatabase.CreateAsset(tileOverlayData, savePath);
            AssetDatabase.SaveAssets();
            tileOverlayData = AssetDatabase.LoadAssetAtPath<TransmutableTileOverlayData>(savePath);
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
                overlayData = tileOverlayData
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
            
            EditorUtils.AssignAddressablesLabel(guid,labels,AssetGroup.Items);
            
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




using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Item.Transmutation;
using Items;
using Items.Transmutable;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

public enum AssetGroup
{
    Items
}

public enum AssetLabel
{
    Item
}
public static class EditorUtils
{
    
    public const string EDITOR_SAVE_PATH = "Assets/EditorCreations";
    public const string TRANSMUTABLE_MATERIAL_PATH = "Assets\\Objects\\TransmutableItems";

    public static string CreateFolder(string folderName)
    {
        string path = Path.Combine("Assets/EditorCreations", folderName);

        if (AssetDatabase.IsValidFolder(path))
        {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path, true);
        }

        AssetDatabase.CreateFolder("Assets/EditorCreations", folderName);
        AssetDatabase.Refresh();
        return path;
    }

    public static void SaveAsset(UnityEngine.Object asset)
    {
        if (!asset) return;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssetIfDirty(asset);
        AssetDatabase.Refresh();
    }
    public static ItemObject GetTransmutableItemObject(TransmutableItemMaterial material, TransmutableItemState state)
    {
        string itemFolder = Path.Combine(TRANSMUTABLE_MATERIAL_PATH, TransmutableItemGenerator.GEN_FOLDER);
        string contentFolder = Path.Combine(itemFolder, material.name);
        string itemPath = Path.Combine(contentFolder, $"{material.name} {state}.asset");
        return AssetDatabase.LoadAssetAtPath<ItemObject>(itemPath);
    }

    public static void AssignAddressablesLabel(string guid, List<AssetLabel> labels, AssetGroup group)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (!asset) return;
        var addressableGroup = settings.FindGroup(GetGroupName(group));
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), addressableGroup);
        foreach (AssetLabel label in labels)
        {
            entry.labels.Add(GetLabelName(label));
        }
        entry?.SetAddress(asset.name); // Assign the address as the asset name
    }

    private static string GetGroupName(AssetGroup group)
    {
        switch (group)
        {
            case AssetGroup.Items:
                return "Items";
            default:
                throw new ArgumentOutOfRangeException(nameof(group), group, null);
        }
    }

    private static string GetLabelName(AssetLabel label)
    {
        switch (label)
        {
            case AssetLabel.Item:
                return "item";
            default:
                throw new ArgumentOutOfRangeException(nameof(label), label, null);
        }
    }
}

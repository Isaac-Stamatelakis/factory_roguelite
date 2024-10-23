using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using Items;
using RecipeModule;
using System.IO;
using TileEntityModule;
using System.Linq;


public enum AddressableTypeRestriction {
    None,
    Item,
    Recipe
}
public class AssignFolderAddressableUtil : EditorWindow {
    private bool reset;
    private string folderPath;
    AddressableAssetGroup group;
    AddressableTypeRestriction typeRestriction;
    [MenuItem("Tools/Addressables/Folder")]
    public static void ShowWindow()
    {
        AssignFolderAddressableUtil window = (AssignFolderAddressableUtil)EditorWindow.GetWindow(typeof(AssignFolderAddressableUtil));
        window.titleContent = new GUIContent("Addressable Folder Assigner");
    }

    void OnGUI()
    {


        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Folder Path:", GUILayout.Width(100));
        folderPath = EditorGUILayout.TextField(folderPath);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        group = EditorGUILayout.ObjectField("Group", group, typeof(AddressableAssetGroup), true) as AddressableAssetGroup;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        typeRestriction = (AddressableTypeRestriction)EditorGUILayout.EnumPopup("Type Restriction", typeRestriction);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        reset = EditorGUILayout.Toggle("Reset", reset);

       
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate"))
        {
            if (reset) {
                resetAddressables();
            } else {
                setFolderAddressable();
            }
            
        }
    }

    void setFolderAddressable() {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        int counter = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            List<string> labels = new List<string>();
            switch (typeRestriction) {
                case AddressableTypeRestriction.None:
                    break;
                case AddressableTypeRestriction.Item:
                    if (asset is not ItemObject) {
                        continue;
                    }
                    labels.Add("item");
                    if (asset is TileItem tileItem) {
                        labels.Add("tile_item");
                        loadTileItem(tileItem, assetPath, settings);
                    }
                    break;
                case AddressableTypeRestriction.Recipe:
                    if (asset is not Recipe) {
                        continue;
                    }
                    labels.Add("recipe");
                    break;
            }
            // Create an addressable entry
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
            foreach (string label in labels) {
                entry.labels.Add(label);
            }
            if (entry == null) {
                continue;
            }
            counter++;
            entry.SetAddress(asset.name); // Assign the address as the asset name
        }

        // Save changes to Addressable settings
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log($"Added {counter} inside {folderPath} as addressable");
    }

    void loadTileItem(TileItem tileItem, string assetPath, AddressableAssetSettings settings) {
        int lastSlashIndex = assetPath.LastIndexOf('/');
        string parentPath = assetPath.Substring(0,lastSlashIndex);
        string[] allAssetPaths = AssetDatabase.FindAssets("", new[] { parentPath })
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .ToArray();

        // Filter out only the immediate children
        List<string> guids = new List<string>();
        foreach (string path in allAssetPaths)
        {
            
            int lastSlashIndex1 = path.LastIndexOf('/');
            string parent = path.Substring(0,lastSlashIndex1);
            if (parent.Equals(parentPath)) {
                guids.Add(AssetDatabase.AssetPathToGUID(path));
            }
        }
        
        List<(string,TileEntity)> tileEntities = new List<(string,TileEntity)>();
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TileEntity>(path);
            if (asset == null) {
                continue;   
            }
            var tuple = (guid,asset);
            tileEntities.Add(tuple);
        }
        
        if (tileEntities.Count == 0) {
            tileItem.tileEntityReference = null;
            return;
        }
        var tileEntityGroup = settings.FindGroup("TileEntities");

        string tileEntityGuid = tileEntities[0].Item1;
        TileEntity tileEntity1 = tileEntities[0].Item2;
        if (tileEntities.Count > 1) {
            Debug.LogWarning($"{tileItem.name} at {assetPath} has {tileEntities.Count} tile enitites within its folder. Assigned reference to {tileEntity1.name}.");
        }
        string tileEntityPath = AssetDatabase.GUIDToAssetPath(tileEntityGuid);
        var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(tileEntityPath), tileEntityGroup);
        entry.SetAddress(tileEntity1.name);
        entry.labels.Add("tile_entity");
        tileItem.tileEntityReference = new UnityEngine.AddressableAssets.AssetReference(tileEntityGuid);
    }

    void resetAddressables() {
        var settings = AddressableAssetSettingsDefaultObject.Settings;

        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        int counter = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));
            // Create an addressable entry
            if (entry != null)
            {
                counter++;
                settings.RemoveAssetEntry(guid);
            }
        }

        // Save changes to Addressable settings
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log($"Removed {counter} inside {folderPath} as addressable");
    }
}

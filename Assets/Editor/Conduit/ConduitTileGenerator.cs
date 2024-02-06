using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class ConduitTileGenerator : EditorWindow {
    private Texture2D texture;
    private string conduitName;
    private ConduitType conduitType;
    [MenuItem("Tools/Item Constructors/Conduit")]
    public static void ShowWindow()
    {
        ConduitTileGenerator window = (ConduitTileGenerator)EditorWindow.GetWindow(typeof(ConduitTileGenerator));
        window.titleContent = new GUIContent("Conduit Item Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Select Texture to Convert", EditorStyles.boldLabel);
        GUILayout.Label("Ensure texture is formatted correctly");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        texture = EditorGUILayout.ObjectField("Sprite", texture, typeof(Texture2D), true) as Texture2D;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Conduit Name:", GUILayout.Width(70));
        conduitName = EditorGUILayout.TextField(conduitName);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type", GUILayout.Width(70));
        conduitType = (ConduitType)EditorGUILayout.EnumPopup("Conduit Type", conduitType);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate"))
        {
            createRuleTile();
        }
    }

    void createRuleTile()
    {
        string path = "Assets/EditorCreations/" + conduitName + "/";
        
        if (AssetDatabase.IsValidFolder(path)) {
            Debug.LogError("Tile Generation for "+  conduitName + "Abanadoned as Folder already exists at EditorCreations");
            return;
        }
        AssetDatabase.CreateFolder("Assets/EditorCreations", conduitName);
        RuleTile ruleTile = EditorRuleTileFactory.from64x64Texture(texture,"Assets/EditorCreations/" + conduitName, conduitName);
        AssetDatabase.CreateAsset(ruleTile, path + "T~" +conduitName + ".asset");
        ConduitItem conduitItem = null;
        switch (conduitType) {
            case ConduitType.Item:
                ResourceConduitItem resourceConduitItem = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem.type = ResourceConduitType.Item;
                conduitItem = resourceConduitItem;
                break;
            case ConduitType.Fluid:
                ResourceConduitItem resourceConduitItem1 = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem1.type = ResourceConduitType.Fluid;
                conduitItem = resourceConduitItem1;
                break;
            case ConduitType.Energy:
                ResourceConduitItem resourceConduitItem2 = ScriptableObject.CreateInstance<ResourceConduitItem>();
                resourceConduitItem2.type = ResourceConduitType.Energy;
                conduitItem = resourceConduitItem2;
                break;
            case ConduitType.Signal:
            conduitItem = ScriptableObject.CreateInstance<SignalConduitItem>();
                break;

        };
        if (conduitItem == null) { // should never get here
            Debug.LogError("Conduit Item Null");
            return;
        }
        conduitItem.name = conduitName;
        conduitItem.ruleTile = ruleTile;
        conduitItem.id = conduitName;
        conduitItem.id = conduitItem.id.ToLower().Replace(" ","_");

        AssetDatabase.CreateAsset(conduitItem, path + conduitItem.name + ".asset");
    }
}

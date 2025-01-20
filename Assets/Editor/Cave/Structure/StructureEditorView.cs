using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WorldModule.Caves;
using Newtonsoft.Json;

/*
[CustomEditor(typeof(Structure))]
public class StructureEditorView : Editor
{
    private GUIStyle textAreaStyle;
    private bool[] foldoutStates;

    private List<Dictionary<string,string>> variantData;
    void OnEnable()
    {
        textAreaStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true,
            richText = true 
        };
    }
    public override void OnInspectorGUI()
    {
        Structure structure = (Structure)target;
        EditorGUI.BeginChangeCheck ();
		EditorGUILayout.GetControlRect (true, 16f, EditorStyles.foldout);
		if (EditorGUI.EndChangeCheck ()) {
			Debug.Log ("fold state updated");
		}
        EditorGUILayout.LabelField("Structure Variants", EditorStyles.boldLabel);
        if (variantData == null) {
            getVariantData(structure);
        }
        for (int i = 0; i < structure.variants.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], "Variant " + (i + 1), true);
            EditorGUILayout.EndHorizontal();
            if (!foldoutStates[i]) {
                continue;
            }
            EditorGUILayout.LabelField("Mutable", EditorStyles.boldLabel);
            structure.variants[i].Frequency = EditorGUILayout.IntField("Frequency", structure.variants[i].Frequency);
            EditorGUILayout.LabelField("Immutable", EditorStyles.boldLabel);
            EditorGUILayout.Vector2IntField("Size", structure.variants[i].Size);
            int height = 200;
            
            Dictionary<string,string> displayData = variantData[i];
            if (displayData == null) {
                EditorGUILayout.LabelField("Data Corrupted", EditorStyles.boldLabel);
                continue;
            }
            foreach (KeyValuePair<string,string> kvp in displayData) {
                EditorGUILayout.LabelField(kvp.Key, EditorStyles.miniBoldLabel);
                EditorGUILayout.TextArea(kvp.Value, textAreaStyle, GUILayout.MinHeight(height));
            }
            
        }
    }

    private void getVariantData(Structure structure) {
        foldoutStates = new bool[structure.variants.Count];
        variantData = new List<Dictionary<string, string>>();
        foreach (StructureVariant variant in structure.variants) {
            WorldTileConduitData worldTileConduitData = null;
            try {
                worldTileConduitData = JsonConvert.DeserializeObject<WorldTileConduitData>(variant.Data);
            } catch (JsonSerializationException) {
                variantData.Add(null);
                continue;
            }
            Dictionary<string,string> displayData = new Dictionary<string, string>();
            displayData["Base Tiles"] = JsonConvert.SerializeObject(worldTileConduitData.baseData);
            displayData["Background Tiles"] = JsonConvert.SerializeObject(worldTileConduitData.backgroundData);
            displayData["Fluid Tiles"] = JsonConvert.SerializeObject(worldTileConduitData.fluidData);
            displayData["Entities"] = JsonConvert.SerializeObject(worldTileConduitData.entityData);
            displayData["Item Conduits"] = JsonConvert.SerializeObject(worldTileConduitData.itemConduitData);
            displayData["Fluid Conduits"] = JsonConvert.SerializeObject(worldTileConduitData.fluidConduitData);
            displayData["Energy Conduits"] = JsonConvert.SerializeObject(worldTileConduitData.energyConduitData);
            displayData["Signal Conduits"] = JsonConvert.SerializeObject(worldTileConduitData.signalConduitData);
            displayData["Matrix Conduits"] = JsonConvert.SerializeObject(worldTileConduitData.matrixConduitData);
            variantData.Add(displayData);
        }
    }

}
*/
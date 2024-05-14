using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WorldModule.Caves;
using Newtonsoft.Json;

[CustomEditor(typeof(Structure))]
public class StructureEditorView : Editor
{
    private GUIStyle textAreaStyle;
    private bool[] foldoutStates;
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

        foldoutStates = new bool[structure.variants.Count];
        EditorGUILayout.LabelField("Structure Variants", EditorStyles.boldLabel);

        for (int i = 0; i < structure.variants.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], "Variant " + (i + 1), true);
            EditorGUILayout.EndHorizontal();
            /*
            if (!foldoutStates[i]) {
                continue;
            }
            */
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Variant " + (i + 1), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Mutable", EditorStyles.boldLabel);
            structure.variants[i].Frequency = EditorGUILayout.IntField("Frequency", structure.variants[i].Frequency);
            EditorGUILayout.LabelField("Immutable", EditorStyles.boldLabel);
            EditorGUILayout.Vector2IntField("Size", structure.variants[i].Size);
            int height = 200;
            WorldTileConduitData worldTileConduitData = null;
            try {
                worldTileConduitData = JsonConvert.DeserializeObject<WorldTileConduitData>(structure.variants[i].Data);
            } catch (JsonSerializationException) {
                
            }
            if (worldTileConduitData == null) {
                EditorGUILayout.LabelField("Data Corrupted", EditorStyles.boldLabel);
                continue;
            }
            EditorGUILayout.LabelField("Base Tiles", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.baseData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Background Tiles", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.backgroundData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Fluid Tiles", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.fluidData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Entities", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.entityData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Item Conduits", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.itemConduitData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Fluid Conduits", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.fluidConduitData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Energy Conduits", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.energyConduitData), textAreaStyle, GUILayout.MinHeight(height));
            
            EditorGUILayout.LabelField("Signal Conduits", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.signalConduitData), textAreaStyle, GUILayout.MinHeight(height));
           
            EditorGUILayout.LabelField("Matrix Conduits", EditorStyles.miniBoldLabel);
            EditorGUILayout.TextArea(JsonConvert.SerializeObject(worldTileConduitData.matrixConduitData), textAreaStyle, GUILayout.MinHeight(height));
            
        }
    }

}

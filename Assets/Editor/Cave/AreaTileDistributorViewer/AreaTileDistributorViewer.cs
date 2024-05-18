using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WorldModule.Caves;

[CustomEditor(typeof(AreaTileDistributor),editorForChildClasses: true)]
public class TileDistributionViewer : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();
        SerializedProperty tileDistributionsProp = serializedObject.FindProperty("tileDistributions");
        for (int i = 0; i < tileDistributionsProp.arraySize; i++) {
            SerializedProperty property = tileDistributionsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.PropertyField(property,true);
            SerializedProperty restrictionModeProp = property.FindPropertyRelative("restriction");
            TilePlacementRestriction mode = (TilePlacementRestriction)restrictionModeProp.enumValueIndex;
            switch (mode) {
                case TilePlacementRestriction.Horizontal:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("minWidth"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("maxWidth"));
                    break;
                case TilePlacementRestriction.Vertical:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("minHeight"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("maxHeight"));
                    break;
                case TilePlacementRestriction.Rectangle:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("minWidth"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("maxWidth"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("minHeight"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("maxHeight"));
                    break;
                case TilePlacementRestriction.Circle:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("minRadius"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("maxRadius"));
                    break;
            }
        }

        if (GUILayout.Button("Add New Distribution")) {
            tileDistributionsProp.InsertArrayElementAtIndex(tileDistributionsProp.arraySize);
            SerializedProperty newElement = tileDistributionsProp.GetArrayElementAtIndex(tileDistributionsProp.arraySize - 1);
        }
        if (GUILayout.Button("Remove Distribution")) {
                tileDistributionsProp.DeleteArrayElementAtIndex(tileDistributionsProp.arraySize-1);
            }
        serializedObject.ApplyModifiedProperties();
        tileDistributionsProp.Dispose();
        
    }

}

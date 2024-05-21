using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Items.Transmutable;
[CustomEditor(typeof(TransmutableItemMaterial),editorForChildClasses: true)]
public class StateOptionViewer : Editor
{
    private bool enableDetailedView = false;
    private SerializedProperty statesProp;
    private void OnEnable() {
        statesProp = serializedObject.FindProperty("states");
    }
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PrefixLabel("Enable Detailed View");
        enableDetailedView = EditorGUILayout.Toggle(enableDetailedView);
        TransmutableItemMaterial material = (TransmutableItemMaterial) target;
        base.OnInspectorGUI();
        /*
        if (!enableDetailedView) {
            EditorGUILayout.PrefixLabel("States", EditorStyles.boldLabel);
            for (int i = 0; i < material.states.Count; i++) {
                material.states[i].state = (TransmutableItemState)EditorGUILayout.EnumPopup($"State {i+1}", material.states[i].state);
            }
        } else {
            for (int i = 0; i < material.states.Count; i++) {
                SerializedProperty myElement = statesProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(myElement,includeChildren:true);
            }
            
            
            
        }
        serializedObject.ApplyModifiedProperties(); 
        */
    }
}

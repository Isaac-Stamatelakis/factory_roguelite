using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Items;
using System.Linq;
using System;

[CustomEditor(typeof(PresetItemObject),editorForChildClasses: true)]
public class PresetItemObjectViewer : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        /*
        PresetItemObject itemObject = (PresetItemObject)target;
        if (itemObject.getDisplayType() == ItemDisplayType.Single) {
            if (itemObject.Sprites == null || itemObject.Sprites.Length < 1) {
                itemObject.Sprites = new Sprite[]{null};
            }
            GUILayout.BeginHorizontal();
            itemObject.Sprites[0] = (Sprite)EditorGUILayout.ObjectField("Sprite", itemObject.Sprites[0], typeof(Sprite), false);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        } else if (itemObject.getDisplayType() == ItemDisplayType.Stack || itemObject.getDisplayType() == ItemDisplayType.Animated) {
            EditorGUILayout.LabelField("Sprites",EditorStyles.boldLabel);
            if (itemObject.Sprites != null)
            {
                for (int i = 0; i < itemObject.Sprites.Length; i++)
                {
                    itemObject.Sprites[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i}", itemObject.Sprites[i], typeof(Sprite), false);
                }
            }
            if (GUILayout.Button("Add Sprite"))
            {
                Sprite[] sprites = new Sprite[itemObject.Sprites.Length+1];
                for (int i = 0; i < itemObject.Sprites.Length; i++) {
                    sprites[i] = itemObject.Sprites[i];
                }
                itemObject.Sprites = sprites;
            }

            if (itemObject.Sprites.Length > 0 && GUILayout.Button("Remove Last Sprite"))
            {
                Sprite[] sprites = new Sprite[itemObject.Sprites.Length-1];
                for (int i = 0; i < sprites.Length; i++) {
                    sprites[i] = itemObject.Sprites[i];
                }
                itemObject.Sprites = sprites;
            }
            
        } 
        */
        
    }
}

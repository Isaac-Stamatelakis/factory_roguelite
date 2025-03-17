using System;
using System.Collections;
using System.Collections.Generic;
using Recipe;
using Recipe.Objects.Generation;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WorldModule.Caves;


[CustomEditor(typeof(RecipeGenerator),editorForChildClasses: false)]
public class RecipeGeneratorInputView : Editor
{
    private SerializedProperty inputProperty;
    private SerializedProperty recipeCollectionProperty;
    private SerializedProperty recipeTypeProperty;
    private SerializedProperty multiplierProperty;
    private SerializedProperty InputAmountProperty;
    private SerializedProperty OutputAmountProperty;
    private SerializedProperty OutputItemProperty;
    
    private ReorderableList inputsList;
    private const float VERTICAL_SPACING = 2f;
    private void OnEnable()
    {
        inputProperty = serializedObject.FindProperty("Inputs");
        recipeCollectionProperty = serializedObject.FindProperty("RecipeCollection");
        recipeTypeProperty = serializedObject.FindProperty("GenerationType");
        multiplierProperty = serializedObject.FindProperty("Multiplier");
        InputAmountProperty = serializedObject.FindProperty("InputAmounts");
        OutputAmountProperty = serializedObject.FindProperty("OutputAmounts");
        OutputItemProperty = serializedObject.FindProperty("Outputs");
        
        inputsList = new ReorderableList(serializedObject, inputProperty, true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Recipe Generation Inputs");
            },
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = inputProperty.GetArrayElementAtIndex(index);
                SerializedProperty inputsProperty = element.FindPropertyRelative("Inputs");
                
                ReorderableList nestedList = new ReorderableList(element.serializedObject, inputsProperty, true, true, true, true)
                {
                    drawHeaderCallback = (Rect nestedRect) =>
                    {
                        EditorGUI.LabelField(nestedRect, $"Input List {index + 1}");
                    },
                    
                    drawElementCallback = (Rect nestedRect, int nestedIndex, bool nestedIsActive, bool nestedIsFocused) =>
                    {
                        SerializedProperty nestedElement = inputsProperty.GetArrayElementAtIndex(nestedIndex);
                        SerializedProperty modeProperty = nestedElement.FindPropertyRelative("Mode");
                        SerializedProperty amountProperty = nestedElement.FindPropertyRelative("Amount");
                        SerializedProperty itemObjectProperty = nestedElement.FindPropertyRelative("ItemObject");
                        SerializedProperty materialProperty = nestedElement.FindPropertyRelative("Material");
                        SerializedProperty itemStateProperty = nestedElement.FindPropertyRelative("ItemState");
                        
                        float lineHeight = EditorGUIUtility.singleLineHeight;
                        
                        nestedRect.height = lineHeight;
                        EditorGUI.LabelField(nestedRect, $"Input {nestedIndex + 1}");
                        
                        nestedRect.y += lineHeight + VERTICAL_SPACING;
                        EditorGUI.PropertyField(nestedRect, modeProperty);
                        
                        nestedRect.y += lineHeight + VERTICAL_SPACING;
                        
                        EditorGUI.PropertyField(nestedRect, amountProperty);
                        nestedRect.y += lineHeight + VERTICAL_SPACING;
                        
                        RecipeGenerationInputMode mode = (RecipeGenerationInputMode)modeProperty.enumValueIndex;
                        if (mode == RecipeGenerationInputMode.Object)
                        {
                            EditorGUI.PropertyField(nestedRect, itemObjectProperty);
                            nestedRect.y += lineHeight + VERTICAL_SPACING;

                        } else if (mode == RecipeGenerationInputMode.Material)
                        {
                            EditorGUI.PropertyField(nestedRect, materialProperty);
                            nestedRect.y += lineHeight + VERTICAL_SPACING;

                            EditorGUI.PropertyField(nestedRect, itemStateProperty);
                        }
                    },
                    
                    elementHeightCallback = (int nestedIndex) =>
                    {
                        SerializedProperty nestedElement = inputsProperty.GetArrayElementAtIndex(nestedIndex);
                        int fields = GetRecipeInputFields(nestedElement);
                        return (EditorGUIUtility.singleLineHeight + VERTICAL_SPACING) * fields;
                    }
                };
                nestedList.DoList(rect);
            },
           
            elementHeightCallback = (int index) =>
            {
                SerializedProperty element = inputProperty.GetArrayElementAtIndex(index);
                SerializedProperty inputsProperty = element.FindPropertyRelative("Inputs");
                int totalFields = 0;
                for (int i = 0; i < inputsProperty.arraySize; i++)
                {
                    SerializedProperty input = inputsProperty.GetArrayElementAtIndex(i);
                    totalFields += GetRecipeInputFields(input);
                }

                const int PADDING = 65;
                return (EditorGUIUtility.singleLineHeight + VERTICAL_SPACING) * totalFields + PADDING;
            }
        };
    }

    private int GetRecipeInputFields(SerializedProperty nestedElementProperty)
    {
        SerializedProperty modeProperty = nestedElementProperty.FindPropertyRelative("Mode");
        RecipeGenerationInputMode mode = (RecipeGenerationInputMode)modeProperty.enumValueIndex;
        int fields = 3;
        if (mode == RecipeGenerationInputMode.Object)
        {
            fields++;
        } else if (mode == RecipeGenerationInputMode.Material)
        {
            fields += 2;
        }
        return fields;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.LabelField("Generates All Recipes",EditorStyles.boldLabel);
        GUILayout.Button("Generate");
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(recipeCollectionProperty);
        EditorGUILayout.PropertyField(recipeTypeProperty);
        RecipeType recipeType = (RecipeType)recipeTypeProperty.enumValueIndex;
        if (recipeType is RecipeType.Machine or RecipeType.Generator)
        {
            if (recipeType == RecipeType.Machine)
            {
                EditorGUILayout.LabelField("Energy Cost Multiplier",EditorStyles.boldLabel);
            } else if (recipeType == RecipeType.Generator)
            {
                EditorGUILayout.LabelField("Energy Production Multiplier");
            }
            EditorGUILayout.PropertyField(multiplierProperty);
        }
        EditorGUILayout.PropertyField(InputAmountProperty);
        EditorGUILayout.PropertyField(OutputAmountProperty);
        EditorGUILayout.PropertyField(OutputItemProperty);
        inputsList.DoLayoutList();

        
        serializedObject.ApplyModifiedProperties();
    }
    
}

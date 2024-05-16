using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WorldModule.Caves;
using LibNoise;

[CustomEditor(typeof(CellularGeneratedArea),editorForChildClasses: true)]
public class CellularGenerationVisualizer : GenerationModelVisualizer
{
    private CellularGeneratedArea model;
    public override void OnInspectorGUI() {
        model = (CellularGeneratedArea)target;
        
        base.OnInspectorGUI();
        
    }

    protected override void elementsBeforeVisualization() {
        switch (model.randomType) {
            case RandomType.Standard:
                showFillPercent();
                break;
            case RandomType.Perlin:
                showOctaves();
                showFrequency();
                showLacunarity();
                showPersistence();
                break;
            case RandomType.Billow:
                showOctaves();
                showFrequency();
                showLacunarity();
                showPersistence();
                break;
            case RandomType.Voronoi:
                showFrequency();
                break;
            case RandomType.RidgedMultifractal:
                showOctaves();
                showFrequency();
                showLacunarity();
                showPersistence();
                break;
            case RandomType.Spheres:
                showFrequency();
                break;
        }
    }

    private void showFrequency() {

        model.frequency = EditorGUILayout.Slider("Frequency", model.frequency, 0.01f, 10f);
    }
    private void showFillPercent() {
        model.fillPercent = EditorGUILayout.Slider("Fill Percent", model.fillPercent, 0f, 1f);
    }
    private void showLacunarity() {
        model.lacunarity = EditorGUILayout.Slider("Lacunarity", model.lacunarity, 0f, 10f);
    }
    private void showPersistence() {
        model.persistence = EditorGUILayout.Slider("Persistance", model.persistence, 0f, 1f);
    }
    private void showOctaves() {
        model.octaveCount = EditorGUILayout.IntSlider("Octaves", model.octaveCount, 1, 10);
    }
    private void showQuality() {
        model.qualityMode = (QualityMode)EditorGUILayout.EnumPopup("Quality Mode", model.qualityMode);
    }
}


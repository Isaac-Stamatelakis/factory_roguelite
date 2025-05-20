using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Items.Transmutable;
using Items;
using System;
using EditorScripts.Tier.Generators;
using Item.GameStage;
using Item.Slot;
using NUnit.Framework;
using Tiles;
using Tiles.Options.Overlay;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.VersionControl;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using World.Cave.Collections;
using Object = UnityEngine.Object;

namespace EditorScripts.Tier
{
    public class TierItemGeneratorWindow : EditorWindow
    {
        private const string TIER_INFO_PATH = "Assets/Objects/TierGenerator/TierDeclariations";
        private const string GEN_PATH = "Assets/Objects/TierGenerator/Generated";
        private const string DEFAULT_VALUES_PATH = "Assets/Objects/TierGenerator/DefaultValues.asset";

        [MenuItem("Tools/Item Constructors/Tiers")]
        public static void ShowWindow()
        {
            TierItemGeneratorWindow window =
                (TierItemGeneratorWindow)EditorWindow.GetWindow(typeof(TierItemGeneratorWindow));
            window.titleContent = new GUIContent("Material Item Generator");
        }

        void OnGUI()
        {
            GUILayout.Label($"Generates items for tiers inside {TIER_INFO_PATH}", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            GUI.enabled = false;
            GUILayout.TextArea(
                "Generates items & recipes for new tiers, generates new items & recipes for existing tiers, updates/deletes existing items & recipes");
            GUI.enabled = true;
            if (GUILayout.Button("Generate Tier Items & Recipes"))
            {
                Generate();
            }
        }

        protected void Generate()
        {
            string[] guids = AssetDatabase.FindAssets("", new[] { TIER_INFO_PATH });
            TierItemGeneratorDefaults defaults = AssetDatabase.LoadAssetAtPath<TierItemGeneratorDefaults>(DEFAULT_VALUES_PATH);
            if (!Directory.Exists(GEN_PATH))
            {
                Directory.CreateDirectory(GEN_PATH);
                AssetDatabase.Refresh();
            }
            Debug.Log("Generating Tier Items & Recipes");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset is not TierItemInfoObject tierItemInfoObject) continue;
                if (!tierItemInfoObject.GameStageObject) continue;
                GenerateMaterialItems(tierItemInfoObject,defaults);
            }
            AssetDatabase.SaveAssets();
            
            Debug.Log("Complete");
        }

        private void GenerateMaterialItems(TierItemInfoObject tierItemInfoObject, TierItemGeneratorDefaults defaults)
        {
            string gameStageName = tierItemInfoObject.GameStageObject.GetGameStageName();
            string tierGenPath = Path.Combine(GEN_PATH, gameStageName);
            if (!Directory.Exists(tierGenPath))
            {
                Debug.Log($"Created folder for {tierItemInfoObject.name}");
                AssetDatabase.CreateFolder(GEN_PATH, gameStageName);
            }

            List<TierItemGenerator> generators = new List<TierItemGenerator>
            {
                CreateGenerator<TieredLadderGenerator>(),
                CreateGenerator<TieredChestGenerator>(),
                CreateGenerator<PlatformGenerator>(),
                CreateGenerator<TorchGenerator>(),
            };

            foreach (TierItemGenerator generator in generators)
            {
                generator.Generate();
            }
            
            return;
            TierItemGenerator CreateGenerator<T>() where T : TierItemGenerator
            {
                return (TierItemGenerator)Activator.CreateInstance(typeof(T), tierItemInfoObject, defaults, tierGenPath);
            }
        }
    }
}






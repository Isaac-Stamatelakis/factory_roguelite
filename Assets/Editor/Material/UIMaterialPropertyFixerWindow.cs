using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Packages.Rider.Editor.Util;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class UIMaterialPropertyFixerWindow : EditorWindow
{
    [MenuItem("Tools/Misc/UIMaterial Properties")]
    
    
    public static void ShowWindow()
    {
        UIMaterialPropertyFixerWindow window = (UIMaterialPropertyFixerWindow)EditorWindow.GetWindow(typeof(UIMaterialPropertyFixerWindow));
        window.titleContent = new GUIContent("UI Material Property Assigner");
    }
    private const string SPRITE_LIT_KEYWORD = "USE_SHAPE_LIGHT_TYPE_0";
    void OnGUI()
    {
        if (GUILayout.Button("Apply Properties"))
        {
            ApplyProperties();
        }
    }

    List<string> SplitJsonObjects(string content)
    {
        List<string> jsonObjects = new List<string>();
        StringBuilder currentJson = new StringBuilder();
        int braceLevel = 0;
        bool inString = false;

        foreach (char c in content)
        {
            if (c == '"' && (currentJson.Length == 0 || currentJson[currentJson.Length - 1] != '\\'))
                inString = !inString;

            if (!inString)
            {
                if (c == '{') braceLevel++;
                if (c == '}') braceLevel--;
            }

            currentJson.Append(c);

            if (braceLevel == 0 && currentJson.Length > 0)
            {
                jsonObjects.Add(currentJson.ToString());
                currentJson.Clear();
            }
        }

        return jsonObjects.Where(j => j.Trim().StartsWith("{")).ToList();
    }
    void ApplyProperties()
    {
        string[] guids = AssetDatabase.FindAssets("t:" + nameof(Material));
        int count = 0;
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (!assetPath.StartsWith("Assets/Material/UI") && !assetPath.StartsWith("Assets/Material/Items"))
            {
                continue;
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (IsSpriteLitShader(material)) continue;
            var shader = material.shader;
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            if (!shaderPath.EndsWith(".shadergraph")) continue;
            if (shaderPath != "Assets/Material/Items/HueShift/TestEditGraph.shadergraph") continue;
            
            string json = File.ReadAllText(shaderPath);
            List<string> splitJson = SplitJsonObjects(json);
            const string PROPERTY_KEY = "m_Properties";
            const string CHILD_KEY = "m_ChildObjectList";
            
            JObject shaderGraph = JObject.Parse(splitJson[0]);
            JArray properties = (JArray)shaderGraph[PROPERTY_KEY];
            
            JObject last = JObject.Parse(splitJson[^1]);
            JArray childrenObjectList = (JArray)last[CHILD_KEY];
            
            HashSet<string> currentProperties = new HashSet<string>();
            for (int i = 1; i < splitJson.Count - 1; i++)
            {
                const string NAME_PROPERTY_KEY = "m_Name";
                JObject jsonObject = JObject.Parse(splitJson[i]);
                JToken propertyName = jsonObject[NAME_PROPERTY_KEY];
                JToken propertyType = jsonObject["m_Type"];
                if (propertyName == null || propertyType?.ToString() != "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty") continue;
                currentProperties.Add(propertyName.ToString());
            }

            List<string> requiredProperties = new List<string>
            {
                "_StencilComp",
                "_Stencil",
                "_StencilOp",
                "_StencilWriteMask",
                "_StencilReadMask"
            };

            foreach (string requiredProperty in requiredProperties)
            {
                if (currentProperties.Contains(requiredProperty)) continue;

                string id = Guid.NewGuid().ToString("N");
                JObject newProperty = new JObject
                {
                    ["m_Id"] = id,
                };
                properties.Add(newProperty);
                childrenObjectList.Add(newProperty);string templateJson = File.ReadAllText("Assets/Editor/Material/TEMPLATE_FLOAT.txt");
                templateJson = templateJson.Replace("$TEMPLATE_ID", id).Replace("$TEMPLATE_NAME", requiredProperty);
                splitJson.Insert(splitJson.Count - 1, templateJson);
                Debug.Log($"Added property {requiredProperty} to {shader.name} with guid {guid}");
            }
            
            splitJson[0] = shaderGraph.ToString(Formatting.Indented);
            splitJson[^1] = last.ToString(Formatting.Indented);
            foreach (string split in splitJson)
            {
                Debug.Log(split);
            }
            //File.WriteAllText(shaderPath, formattedJson);
            break;
            Debug.Log(shader.name);
        }

        bool IsSpriteLitShader(Material material)
        {
            if (!material.shader) return false;
            foreach (string keyword in material.shader.keywordSpace.keywordNames)
            {
                if (keyword == SPRITE_LIT_KEYWORD)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

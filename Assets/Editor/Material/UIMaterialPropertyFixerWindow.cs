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
            JObject newProperty = new JObject
            {
                ["m_Id"] = "Test",
            };
            
            properties.Add(newProperty);

            // Write back formatted JSON
            string formattedJson = shaderGraph.ToString(Formatting.Indented);
            Debug.Log(formattedJson);
            
            JObject last = JObject.Parse(splitJson[^1]);
            JArray childrenObjectList = (JArray)last[CHILD_KEY];
            childrenObjectList.Add(newProperty);
            string formattedJson1 = childrenObjectList.ToString(Formatting.Indented);
            Debug.Log(formattedJson1);
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

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class WorldSelectUI : EditorWindow
{
    [MenuItem("Window/UI Toolkit/WorldSelectUI")]
    public static void ShowExample()
    {
        WorldSelectUI wnd = GetWindow<WorldSelectUI>();
        wnd.titleContent = new GUIContent("WorldSelectUI");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/WorldSelect/WorldSelectUI.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
    }
}
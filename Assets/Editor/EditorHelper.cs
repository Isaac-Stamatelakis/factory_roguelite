using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public static class EditorHelper
{
    public static readonly string EDITOR_SAVE_PATH = "Assets/EditorCreations";

    public static string CreateFolder(string folderName)
    {
        string path = Path.Combine("Assets/EditorCreations", folderName);

        if (AssetDatabase.IsValidFolder(path))
        {
            Debug.LogWarning("Replaced existing content at " + path);
            Directory.Delete(path, true);
        }

        AssetDatabase.CreateFolder("Assets/EditorCreations", folderName);
        AssetDatabase.Refresh();
        return path;
    }
}

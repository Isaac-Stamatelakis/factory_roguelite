using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace DevTools {
    public enum DevTool
    {
        Structure = 0,
        QuestBook = 1,
        Upgrade = 2,
        ImageGenerator = 3,
        CraftingTree = 4,
        
    }
    public static class DevToolUtils
    {
        public const string SCENE_NAME = "DevTools";
        public static bool OnDevToolScene => SceneManager.GetSceneByName(SCENE_NAME).isLoaded;
        public static string GetDevToolPath(DevTool devTool)
        {
            string parentFolder = devTool == DevTool.CraftingTree ? Application.dataPath : Application.streamingAssetsPath;
            string devPath = Path.Combine(parentFolder,devTool.ToString());
            Debug.Log(devPath);
            if (!Directory.Exists(devPath)) {
                Directory.CreateDirectory(devPath);
            }
            return devPath;
        }
        
    }
}


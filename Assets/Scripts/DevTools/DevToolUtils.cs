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
        
    }
    public static class DevToolUtils
    {
        public const string SCENE_NAME = "DevTools";
        public static bool OnDevToolScene => SceneManager.GetSceneByName(SCENE_NAME).isLoaded;
        public static string GetDevToolPath(DevTool devTool) {
            string devPath = Path.Combine(Application.streamingAssetsPath,devTool.ToString());
            if (!Directory.Exists(devPath)) {
                Directory.CreateDirectory(devPath);
            }
            return devPath;
        }
        
    }
}


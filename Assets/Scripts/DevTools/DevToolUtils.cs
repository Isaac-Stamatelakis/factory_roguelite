using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DevTools {
    public enum DevTool
    {
        Structure,
        QuestBook
    }
    public static class DevToolUtils
    {
        private readonly static string devToolSaveFolder = "DeveloperTools";

        public static string DevToolSaveFolder => devToolSaveFolder;

        public static string GetDevToolPath(DevTool devTool) {
            string devPath = Path.Combine("Assets","Data",devTool.ToString());
            if (!Directory.Exists(devPath)) {
                Directory.CreateDirectory(devPath);
            }
            return devPath;
        }
    }
}


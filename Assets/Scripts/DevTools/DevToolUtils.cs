using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DevTools {
    public static class DevToolUtils
    {
        private readonly static string devToolSaveFolder = "DeveloperTools";

        public static string DevToolSaveFolder => devToolSaveFolder;

        public static string getDevPath() {
            string devPath = Path.Combine(Application.persistentDataPath,DevToolUtils.DevToolSaveFolder);
            if (!Directory.Exists(devPath)) {
                Directory.CreateDirectory(devPath);
            }
            return devPath;
        }
    }
}


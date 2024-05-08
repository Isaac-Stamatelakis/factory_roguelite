using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldModule {
    public class WorldManager
    {
        private static WorldManager instance;
        private string worldPath;
        private WorldManager() {
            worldPath = "worlds/world0"; // Default
        }
        public static WorldManager getInstance()
        {
            {
                if (instance == null)
                {
                    instance = new WorldManager();
                }
                return instance;
            }
        }
        public void setWorldPath(string path) {
            worldPath = path;
        }
        public string getWorldPath() {
            return worldPath;
        }
    }
}


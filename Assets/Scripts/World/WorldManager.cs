using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevTools;
using Item.GameStage;
using Newtonsoft.Json;
using UI;
using UI.QuestBook;
using UnityEngine;
using World.Serialization;
using Object = UnityEngine.Object;

namespace WorldModule {
    public class WorldManager
    {
        public enum WorldType
        {
            Default,
            Structure
        }
        private static WorldManager instance;
        private string worldName;
        private WorldMetaData metaData;
        public WorldType WorldLoadType = WorldType.Default;
        private WorldManager() {
            worldName = "world0"; // Default
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
        public void SetWorldName(string path) {
            worldName = path;
            
        }

        public void InitializeMetaData()
        {
            string metaDataPath = WorldLoadUtils.GetMetaDataPath(worldName);
            if (!File.Exists(metaDataPath))
            {
                WorldCreation.InitializeMetaData(metaDataPath);
            }
            metaData = WorldLoadUtils.GetWorldMetaData(worldName);
        }

        public void SaveMetaData()
        {
            metaData.LastAccessDate = DateTime.Now;
            WorldLoadUtils.WriteMetaData(worldName, metaData);
        }
        
        public string GetWorldName() {
            return worldName;
        }
    }
}


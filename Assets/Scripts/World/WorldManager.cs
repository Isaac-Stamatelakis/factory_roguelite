using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Item.GameStage;
using Newtonsoft.Json;
using UnityEngine;
using World.Serialization;

namespace WorldModule {
    public class WorldManager
    {
        private static WorldManager instance;
        private string worldPath;
        private WorldMetaData metaData;
        private HashSet<string> unlockedGameStages = new HashSet<string>();
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

        public void InitializeMetaData()
        {
            string metaDataPath = WorldLoadUtils.GetWorldFilePath(WorldFileType.Meta);
            if (!File.Exists(metaDataPath))
            {
                WorldCreation.InitializeMetaData(metaDataPath);
            }
            metaData = JsonConvert.DeserializeObject<WorldMetaData>(File.ReadAllText(metaDataPath));
            unlockedGameStages = new HashSet<string>();
            foreach (string gameStageId in metaData.UnlockedGameStages)
            {
                unlockedGameStages.Add(gameStageId);
            }
        }

        public void SaveMetaData()
        {
            metaData.LastAccessDate = DateTime.Now;
            metaData.UnlockedGameStages = unlockedGameStages.ToList();
            string json = JsonConvert.SerializeObject(metaData);
            string metaDataPath = WorldLoadUtils.GetWorldFilePath(WorldFileType.Meta);
            File.WriteAllText(metaDataPath, json);
        }

        public void UnlockGameStage(GameStageObject gameStageObject)
        {
            UnlockGameStage(gameStageObject.GetGameStageId());
        }
        
        public void UnlockGameStage(string id)
        {
            unlockedGameStages.Add(id);
        }

        public void RemoveGameStage(string id)
        {
            unlockedGameStages.Remove(id);
        }

        public bool HasGameStage(GameStageObject gameStageObject)
        {
            string id = gameStageObject.GetGameStageId();
            return unlockedGameStages.Contains(id);
        }
        public string getWorldPath() {
            return worldPath;
        }
    }
}


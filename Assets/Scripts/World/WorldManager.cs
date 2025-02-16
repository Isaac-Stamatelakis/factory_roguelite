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
        private HashSet<string> unlockedGameStages = new HashSet<string>();
        private QuestBookLibrary questBookLibrary;
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
            unlockedGameStages = new HashSet<string>();
            foreach (string gameStageId in metaData.UnlockedGameStages)
            {
                unlockedGameStages.Add(gameStageId);
            }
        }

        public void SetQuestBookFromJson(string json)
        {
            questBookLibrary = QuestBookLibraryFactory.Deseralize(json);
            QuestBookUIManager.Instance.Initialize(questBookLibrary);
        }

        public void InitializeQuestBook()
        {
            string questBookJson = WorldLoadUtils.GetWorldFileJson(WorldFileType.Questbook);
            SetQuestBookFromJson(questBookJson);
            
            
            
        }

        public void SaveMetaData()
        {
            metaData.LastAccessDate = DateTime.Now;
            metaData.UnlockedGameStages = unlockedGameStages.ToList();
            WorldLoadUtils.WriteMetaData(worldName, metaData);
        }

        public void SaveQuestBook()
        {
            string json = QuestBookLibraryFactory.Serialize(questBookLibrary);
            if (json == null) return;
            WorldLoadUtils.SaveWorldFileJson(WorldFileType.Questbook,json);
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
        public string GetWorldName() {
            return worldName;
        }
    }
}


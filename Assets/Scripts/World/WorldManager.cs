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
        
        public void InitializeQuestBook()
        {
            string mainLibPath = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), QuestBookUtils.MAIN_QUEST_BOOK_NAME);
            string playerQuestBookPath = Path.Combine(WorldLoadUtils.GetMainPath(worldName), QuestBookUtils.WORLD_QUEST_FOLDER_PATH);
            QuestBookUtils.VerifyIntegrityOfQuestBookData(mainLibPath,playerQuestBookPath);
            
        }

        public void SaveMetaData()
        {
            metaData.LastAccessDate = DateTime.Now;
            metaData.UnlockedGameStages = unlockedGameStages.ToList();
            WorldLoadUtils.WriteMetaData(worldName, metaData);
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


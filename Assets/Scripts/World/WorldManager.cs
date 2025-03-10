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

        public WorldMetaData GetMetaData()
        {
            string metaDataPath = WorldLoadUtils.GetMetaDataPath(worldName);
            if (!File.Exists(metaDataPath))
            {
                WorldCreation.InitializeMetaData(metaDataPath,new WorldCreationData(worldName,null,false,QuestBookUtils.MAIN_QUEST_BOOK_NAME));
                return WorldLoadUtils.GetWorldMetaData(worldName);
            }
            WorldMetaData worldMetaData = WorldLoadUtils.GetWorldMetaData(worldName);
            bool error = false;
            if (worldMetaData.QuestBook == null)
            {
                worldMetaData.QuestBook = QuestBookUtils.MAIN_QUEST_BOOK_NAME;
                error = true;
            }
            if (error)
            {
                WorldLoadUtils.WriteMetaData(worldName, worldMetaData);
            }
            return worldMetaData;
        }

        public void SaveMetaData()
        {
            WorldMetaData metaData = WorldLoadUtils.GetWorldMetaData(worldName);
            metaData.LastAccessDate = DateTime.Now;
            WorldLoadUtils.WriteMetaData(worldName, metaData);
        }
        
        public string GetWorldName() {
            return worldName;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevTools;
using Newtonsoft.Json;
using UI.QuestBook.Data;
using UI.QuestBook.Data.Node;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookUtils
    {
        public const string WORLD_QUEST_FOLDER_PATH = "QuestBook";
        public const string MAIN_QUEST_BOOK_NAME = "_main";
        public const string LIBRARY_DATA_PATH = "library_data.json";
        public const string QUESTBOOK_DATA_PATH = "questbook_data.json";
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
        public const bool SHOW_ALL_COMPLETED = false;

        private struct QuestBookVerifyInfo
        {
            public int QuestFoldersAdded;
            public int QuestFoldersDeleted;
            public int PagesAdded;
            public int PagesDeleted;
            public int TasksDeleted;
            public int TasksAdded;
            public override string ToString()
            {
                return $"Player Quest Book Data Verified: " +
                       $"QuestBooks: +{QuestFoldersAdded} & -{QuestFoldersDeleted}, " +
                       $"Pages: +{PagesAdded} & -{PagesDeleted}, " +
                       $"Tasks: +{TasksAdded} & -{TasksDeleted}.";
            }
        }
        public static void VerifyIntegrityOfQuestBookData(string questBookLibraryPath, string playerQuestBookDataPathRoot)
        {
            string questBookLibraryJson = File.ReadAllText(Path.Combine(questBookLibraryPath,LIBRARY_DATA_PATH));
            QuestBookLibraryData questBookLibraryData = JsonConvert.DeserializeObject<QuestBookLibraryData>(questBookLibraryJson);
            Dictionary<string, QuestBookSelectorData> idSelectorDataDict = new Dictionary<string, QuestBookSelectorData>();
            foreach (QuestBookSelectorData selectorData in questBookLibraryData.QuestBookDataList)
            {
                idSelectorDataDict[selectorData.Id] = selectorData;
            }

            if (!Directory.Exists(playerQuestBookDataPathRoot))
            {
                Directory.CreateDirectory(playerQuestBookDataPathRoot);
            }
            
            string[] playerQuestFolders = Directory.GetDirectories(playerQuestBookDataPathRoot);
            QuestBookVerifyInfo questBookVerifyInfo = new QuestBookVerifyInfo();
            foreach (string playerQuestFolder in playerQuestFolders)
            {
                string directoryName = Path.GetFileName(playerQuestFolder);
                
                if (!idSelectorDataDict.ContainsKey(directoryName))
                {
                    Directory.Delete(playerQuestFolder, true);
                    questBookVerifyInfo.QuestFoldersDeleted++;
                }
            }

            foreach (var (questBookId, selectorData) in idSelectorDataDict)
            {
                string playerQuestBookDataPath = Path.Combine(playerQuestBookDataPathRoot, questBookId);
                string questBookFolderPath = Path.Combine(questBookLibraryPath, selectorData.Id);
                if (!playerQuestFolders.Contains(playerQuestBookDataPath))
                {
                    Directory.CreateDirectory(playerQuestBookDataPath);
                    questBookVerifyInfo.QuestFoldersAdded++;
                }

                string questBookJson = File.ReadAllText(Path.Combine(questBookFolderPath, QUESTBOOK_DATA_PATH));
                QuestBookData questBookData = JsonConvert.DeserializeObject<QuestBookData>(questBookJson);
                Dictionary<string, QuestBookPageData> idPageDataDictionary = new Dictionary<string, QuestBookPageData>();
                foreach (QuestBookPageData questBookPageData in questBookData.PageDataList)
                {
                    idPageDataDictionary[questBookPageData.Id] = questBookPageData;
                }
                string[] pageFiles = Directory.GetFiles(playerQuestBookDataPath);
                foreach (string pageFile in pageFiles)
                {
                    string fileName = Path.GetFileName(pageFile).Replace(".json","");
                    if (!idPageDataDictionary.ContainsKey(fileName))
                    {
                        File.Delete(pageFile);
                        questBookVerifyInfo.PagesDeleted++;
                    }
                }

                foreach (var (pageId, pageData) in idPageDataDictionary)
                {
                    string pageFilePath = Path.Combine(playerQuestBookDataPath, pageId) + ".json";
                    List<QuestBookTaskData> taskDataList;
                    if (!pageFiles.Contains(pageFilePath))
                    {
                        taskDataList = new List<QuestBookTaskData>();
                    }
                    else
                    {
                        string taskDataJson =  File.ReadAllText(pageFilePath);
                        taskDataList = JsonConvert.DeserializeObject<List<QuestBookTaskData>>(taskDataJson);
                    }
    
                    Dictionary<int, QuestBookTaskData> questBookNodeDataDict = new Dictionary<int, QuestBookTaskData>();
                    foreach (QuestBookTaskData taskData in taskDataList)
                    {
                        questBookNodeDataDict[taskData.Id] = taskData;
                    }
                    List<QuestBookNodeData> questBookNodeDataList = QuestBookFactory.GetQuestBookPageNodeData(questBookFolderPath, pageId);
                    if (questBookNodeDataList == null) continue;

                    HashSet<int> validNodes = new HashSet<int>();
                    
                    foreach (QuestBookNodeData questBookNodeData in questBookNodeDataList)
                    {
                        if (!questBookNodeDataDict.ContainsKey(questBookNodeData.Id))
                        {
                            taskDataList.Add( new QuestBookTaskData(false, new QuestBookRewardClaimStatus(), questBookNodeData.Id));
                            questBookVerifyInfo.TasksAdded++;
                        }
                        validNodes.Add(questBookNodeData.Id);
                    }

                    for (var index = taskDataList.Count - 1; index >= 0; index--)
                    {
                        var taskData = taskDataList[index];
                        if (!validNodes.Contains(taskData.Id))
                        {
                            taskDataList.RemoveAt(index);
                            questBookVerifyInfo.TasksDeleted++;
                        }
                    }

                    if (!File.Exists(pageFilePath))
                    {
                        questBookVerifyInfo.PagesAdded++;
                    }
                    string json = JsonConvert.SerializeObject(taskDataList);
                    File.WriteAllText(pageFilePath, json);
                }
            }
        }
    }
}


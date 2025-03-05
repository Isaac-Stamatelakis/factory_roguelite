using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DevTools;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookUtils
    {
        public const string WORLD_QUEST_FOLDER_PATH = "QuestBook";
        public const string MAIN_QUEST_BOOK_NAME = "_main";
        public const string LIBRARY_DATA_PATH = "library_data.bin";
        public const string QUESTBOOK_DATA_PATH = "questbook_data.bin";
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
        public const bool SHOW_ALL_COMPLETED = false;

        public static void VerifyIntegrityOfQuestBookData(string questBookLibraryPath, string playerQuestBookDataPathRoot)
        {
            QuestBookLibraryData questBookLibraryData = GlobalHelper.DeserializeCompressedJson<QuestBookLibraryData>(Path.Combine(questBookLibraryPath,LIBRARY_DATA_PATH));
            Dictionary<string, QuestBookSelectorData> idSelectorDataDict = new Dictionary<string, QuestBookSelectorData>();
            foreach (QuestBookSelectorData selectorData in questBookLibraryData.QuestBookDataList)
            {
                idSelectorDataDict[selectorData.Id] = selectorData;
            }
            string[] playerQuestFolders = Directory.GetDirectories(playerQuestBookDataPathRoot);
            
            foreach (string playerQuestFolder in playerQuestFolders)
            {
                string directoryName = Path.GetDirectoryName(playerQuestFolder);
                Debug.Log(directoryName);
                /*
                if (!idSelectorDataDict.ContainsKey(directoryName))
                {
                    Directory.Delete(playerQuestFolder, true);
                }
                */
            }

            foreach (var (questBookId, selectorData) in idSelectorDataDict)
            {
                string playerQuestBookDataPath = Path.Combine(playerQuestBookDataPathRoot, questBookId);
                string questBookFolderPath = Path.Combine(questBookLibraryPath, selectorData.Id);
                if (!playerQuestFolders.Contains(playerQuestBookDataPath))
                {
                    Directory.CreateDirectory(playerQuestBookDataPath);
                }
                QuestBookData questBookData = GlobalHelper.DeserializeCompressedJson<QuestBookData>(Path.Combine(questBookFolderPath,QUESTBOOK_DATA_PATH));
                Dictionary<string, QuestBookPageData> idPageDataDictionary = new Dictionary<string, QuestBookPageData>();
                foreach (QuestBookPageData questBookPageData in questBookData.PageDataList)
                {
                    idPageDataDictionary[questBookPageData.Id] = questBookPageData;
                }
                string[] pageFiles = Directory.GetFiles(playerQuestBookDataPath);
                foreach (string pageFile in pageFiles)
                {
                    string fileName = Path.GetFileName(pageFile).Replace(".bin","");
                    if (!idPageDataDictionary.ContainsKey(pageFile))
                    {
                        File.Delete(fileName);
                    }
                }

                foreach (var (pageId, pageData) in idPageDataDictionary)
                {
                    string pageFillPath = Path.Combine(playerQuestBookDataPath, pageId) + ".bin";
                    List<QuestBookTaskData> taskDataList;
                    if (!pageFiles.Contains(pageFillPath))
                    {
                        taskDataList = new List<QuestBookTaskData>();
                    }
                    else
                    {
                        taskDataList = GlobalHelper.DeserializeCompressedJson<List<QuestBookTaskData>>(pageFillPath);
                    }
    
                    Dictionary<int, QuestBookTaskData> questBookNodeDataDict = new Dictionary<int, QuestBookTaskData>();
                    foreach (QuestBookTaskData taskData in taskDataList)
                    {
                        questBookNodeDataDict[taskData.Id] = taskData;
                    }
                    List<QuestBookNodeData> questBookNodeDataList = QuestBookLibraryFactory.GetQuestBookPageNodeData(questBookFolderPath, pageId);
                    if (questBookNodeDataList == null) continue;

                    HashSet<int> validNodes = new HashSet<int>();
                    
                    foreach (QuestBookNodeData questBookNodeData in questBookNodeDataList)
                    {
                        if (!questBookNodeDataDict.ContainsKey(questBookNodeData.Id))
                        {
                            taskDataList.Add( new QuestBookTaskData(false, new QuestBookRewardClaimStatus(), questBookNodeData.Id));
                        }
                        validNodes.Add(questBookNodeData.Id);
                    }

                    for (var index = taskDataList.Count - 1; index >= 0; index--)
                    {
                        var taskData = taskDataList[index];
                        if (!validNodes.Contains(taskData.Id))
                        {
                            taskDataList.RemoveAt(index);
                        }
                    }
                    GlobalHelper.SerializeCompressedJson(taskDataList, pageFillPath);
                }
            }
            
            
            
        }
    }
}


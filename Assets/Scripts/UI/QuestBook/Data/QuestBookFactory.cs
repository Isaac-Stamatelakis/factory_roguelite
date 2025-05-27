using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UI.QuestBook.Data.Node;
using UnityEngine;
using WorldModule;

namespace UI.QuestBook.Data {
    public static class QuestBookFactory {
        public static QuestBookLibraryData GetDefaultLibraryData()
        {

            List<QuestBookSelectorData> questBookSelectorDataList = new List<QuestBookSelectorData>
            {
                new QuestBookSelectorData("A Dummies Guide to Portal Creation", QuestBookTitleSpritePath.Stars, GlobalHelper.GenerateHash())
            };
            return new QuestBookLibraryData(questBookSelectorDataList);
        }

        private static SerializedQuestBookNode SerializeNodeData(QuestBookNodeData questBookNodeData) {
            return new SerializedQuestBookNode(
                questBookNodeData,
                SerializeTask(questBookNodeData.Content.Task)
            );
        }
        
        public static List<QuestBookNodeData> GetQuestBookPageNodeData(string questBookDirectory, string questBookPageID)
        {
            string pagePath = Path.Combine(questBookDirectory, questBookPageID) + ".json";
            if (!File.Exists(pagePath))
            {
                throw new InvalidQuestBookException($"Could not find quest book page node ata at {pagePath}");
            }

            try
            {
                string json = File.ReadAllText(pagePath);
                List<SerializedQuestBookNode> serializedQuestBookNodes = JsonConvert.DeserializeObject<List<SerializedQuestBookNode>>(json);
                List<QuestBookNodeData> questBookNodeDataList = new List<QuestBookNodeData>();
                foreach (var serializedQuestBookNode in serializedQuestBookNodes)
                {
                    QuestBookNodeData nodeData = DeserializeNode(serializedQuestBookNode);
                    if (nodeData == null) continue;
                    questBookNodeDataList.Add(nodeData);
                }
                return questBookNodeDataList;
            }
            catch (JsonSerializationException e)
            {
                throw new InvalidQuestBookException(e.Message);
            }
        }
        

        public static QuestBookPage GetQuestBookPage(List<QuestBookNodeData> nodeDataList, List<QuestBookTaskData> taskDataList)
        {
            Dictionary<int, QuestBookTaskData> idNodeDataDictionary = new Dictionary<int, QuestBookTaskData>();
            
            
            foreach (QuestBookTaskData taskData in taskDataList)
            {
                idNodeDataDictionary[taskData.Id] = taskData;
            }
            
            List<QuestBookNode> questBookNodes = new List<QuestBookNode>();
            foreach (QuestBookNodeData questBookNodeData in nodeDataList)
            {
                QuestBookTaskData questBookTask = idNodeDataDictionary.GetValueOrDefault(questBookNodeData.Id);
                if (questBookTask == null)
                {
                    questBookTask = new QuestBookTaskData(false,new QuestBookRewardClaimStatus(),questBookNodeData.Id);
                }
                QuestBookNode questBookNode = new QuestBookNode(questBookNodeData,questBookTask);
                questBookNodes.Add(questBookNode);
            }
            return new QuestBookPage(questBookNodes);
        }

        public static void SerializedQuestBookNodeData(string questBookDirectory, string questBookPageID, List<QuestBookNodeData> questBookNodeDataList)
        {
            List<SerializedQuestBookNode> serializedQuestBookNodes = new List<SerializedQuestBookNode>();
            foreach (QuestBookNodeData questBookNodeData in questBookNodeDataList)
            {
                SerializedQuestBookNode serializedQuestBookNode = SerializeNodeData(questBookNodeData);
                if (serializedQuestBookNode == null) continue;
                serializedQuestBookNodes.Add(serializedQuestBookNode);
            }
            string json = JsonConvert.SerializeObject(serializedQuestBookNodes);
            string savePath = Path.Combine(questBookDirectory, questBookPageID);
            savePath += ".json";
            File.WriteAllText(savePath, json);
        }
        

        public static QuestBookData GetDefaultQuestBookData()
        {
            return new QuestBookData(0, new List<QuestBookPageData>());
        }
        
        private static QuestBookNodeData DeserializeNode(SerializedQuestBookNode node)
        {
            QuestBookNodeData nodeData = node.QuestBookNodeData;
            nodeData.Content.Task = DeserializeTask(node.SerializedQuestBookTaskData);
            return nodeData;
        }
        
        
        private static QuestBookTask DeserializeTask(SerializedQuestBookTaskData serializedQuestBookTaskData) {
            switch (serializedQuestBookTaskData.QuestTaskType) {
                case QuestTaskType.Item:
                    return JsonConvert.DeserializeObject<ItemQuestTask>(serializedQuestBookTaskData.TaskData);
                case QuestTaskType.Checkmark:
                    return JsonConvert.DeserializeObject<CheckMarkQuestTask>(serializedQuestBookTaskData.TaskData);
                case QuestTaskType.Dimension:
                    return JsonConvert.DeserializeObject<VisitDimensionQuestTask>(serializedQuestBookTaskData.TaskData);
                default:
                    throw new ArgumentOutOfRangeException(nameof(serializedQuestBookTaskData));
            }
        }

        private static SerializedQuestBookTaskData SerializeTask(QuestBookTask questBookTask)
        {
            QuestTaskType taskType = questBookTask.GetTaskType();
            return new SerializedQuestBookTaskData(taskType, JsonConvert.SerializeObject(questBookTask));

        }
        
        private class SerializedQuestBookNode
        {
            public QuestBookNodeData QuestBookNodeData;
            public SerializedQuestBookTaskData SerializedQuestBookTaskData;
            public SerializedQuestBookNode(QuestBookNodeData questBookNodeData, SerializedQuestBookTaskData serializedQuestBookTaskData) {
                this.QuestBookNodeData = questBookNodeData;
                this.SerializedQuestBookTaskData = serializedQuestBookTaskData;
                
            }
        }
        private class SerializedQuestBookTaskData
        {
            public QuestTaskType QuestTaskType;
            public string TaskData;

            public SerializedQuestBookTaskData(QuestTaskType questTaskType, string taskData)
            {
                QuestTaskType = questTaskType;
                TaskData = taskData;
            }
        }
    }
}


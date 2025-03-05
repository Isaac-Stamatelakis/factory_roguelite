using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using TileEntity.Instances.CompactMachines;
using UI.QuestBook.Tasks;
using WorldModule;

namespace UI.QuestBook {
    public class InvalidQuestBookException : Exception
    {
        public InvalidQuestBookException(string message) : base(message)
        {
        }
    }
    public enum QuestBookTitleSpritePath
    {
        Stars = 0,
        BluePlanet = 1,
        PurplePlanet = 2,
        Teleporter = 3
    }
    /// <summary>
    /// A collection of quest books
    /// </summary>
    public class QuestBookLibrary
    {
        public List<QuestBook> QuestBooks;
        [JsonIgnore] public Dictionary<int, QuestBookNode> IdNodeMap { get => idNodeMap;}
        [JsonIgnore] private Dictionary<int, QuestBookNode> idNodeMap;
        
        public QuestBookLibrary(List<QuestBook> books) {
            this.QuestBooks = books;
            InitializeIdNodeMap();
        }
        public void InitializeIdNodeMap() {
            idNodeMap = new Dictionary<int, QuestBookNode>();
            foreach (QuestBook questBook in QuestBooks) {
                foreach (QuestBookPage page in questBook.Pages) {
                    foreach (QuestBookNode node in page.Nodes) {
                        if (idNodeMap.TryGetValue(node.Id, out var value)) {
                            Debug.LogWarning("Nodes " + node.Content.Title + " and " + value.Content.Title+ " have duplicate id:" + node.Id);
                            continue;
                        }
                        idNodeMap[node.Id] = node;
                    }
                }
            }
        }

        public int GetSmallestNewID() {
            int smallestNewID = 0;
            while (idNodeMap.ContainsKey(smallestNewID)) {
                smallestNewID++;
            }
            return smallestNewID;
        }

        public void AddNode(QuestBookNode node) {
            idNodeMap[node.Id] = node;
        }

        public QuestBookNode GetNode(int id) {
            if (idNodeMap.ContainsKey(id)) {
                return idNodeMap[id];
            }
            return null;
        }
        public void RemoveNode(QuestBookNode node) {
            if (idNodeMap.ContainsKey(node.Id)) {
                idNodeMap.Remove(node.Id);
            }
        }
    }
    
    public static class QuestBookLibraryFactory {
        
        private const string QUEST_BOOK_DATA_FILE = "quest_book_data.bin";
        
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

        
        public static QuestBookData GetQuestBookData(string questBookDirectory) {
            string questBookDataPath = Path.Combine(questBookDirectory, QUEST_BOOK_DATA_FILE);
            if (!File.Exists(questBookDataPath))
            {
                return GetDefaultQuestBookData();
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(questBookDataPath);
                string json = WorldLoadUtils.DecompressString(bytes);
                return JsonConvert.DeserializeObject<QuestBookData>(json);
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError(e.Message);
                return GetDefaultQuestBookData();
            }
        }

        public static List<QuestBookNodeData> GetQuestBookPageNodeData(string questBookDirectory, string questBookPageID)
        {
            string pagePath = Path.Combine(questBookDirectory, questBookPageID) + ".bin";
            if (!File.Exists(pagePath))
            {
                throw new InvalidQuestBookException($"Could not find quest book page node ata at {pagePath}");
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(pagePath);
                string json = WorldLoadUtils.DecompressString(bytes);
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
            Dictionary<int,QuestBookNode> idNodeDictionary = new Dictionary<int, QuestBookNode>();
            foreach (QuestBookNodeData questBookNodeData in nodeDataList)
            {
                QuestBookTaskData questBookTask = idNodeDataDictionary.GetValueOrDefault(questBookNodeData.Id);
                if (questBookTask == null)
                {
                    questBookTask = new QuestBookTaskData(false,new QuestBookRewardClaimStatus(),questBookNodeData.Id);
                }
                QuestBookNode questBookNode = new QuestBookNode(questBookNodeData,questBookTask);
                questBookNodes.Add(questBookNode);
                idNodeDictionary[questBookNodeData.Id] = questBookNode;
            }
            return new QuestBookPage(questBookNodes,idNodeDictionary);
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
            byte[] bytes = WorldLoadUtils.CompressString(json);
            string savePath = Path.Combine(questBookDirectory, questBookPageID);
            savePath += ".bin";
            File.WriteAllBytes(savePath, bytes);
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
        

        private class SerializedQuestBookPage {
            public string title;
            public List<SerializedQuestBookNode> nodes;
            public SerializedQuestBookPage(string title, List<SerializedQuestBookNode> nodes) {
                this.title = title;
                this.nodes = nodes;
            }
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
    
    public class QuestBookLibraryData
    {
        public List<QuestBookSelectorData> QuestBookDataList;

        public QuestBookLibraryData(List<QuestBookSelectorData> questBookDataList)
        {
            QuestBookDataList = questBookDataList;
        }

        public object Path { get; set; }
    }

    public class QuestBookSelectorData
    {
        public string Title;
        public QuestBookTitleSpritePath SpritePath;
        public string Id;

        public QuestBookSelectorData(string title, QuestBookTitleSpritePath spritePath, string id)
        {
            Title = title;
            SpritePath = spritePath;
            Id = id;
        }
    }
    
    public class QuestBookData {
        public int IDCounter;
        public List<QuestBookPageData> PageDataList;
        public QuestBookData(int idCounter, List<QuestBookPageData> pageDataList) {
            this.IDCounter = idCounter;
            this.PageDataList = pageDataList;
        }
    }
    
    public class QuestBookPageData
    {
        public string Title;
        public string Id;

        public QuestBookPageData(string title, string id)
        {
            Title = title;
            Id = id;
        }
    }
    
    

}


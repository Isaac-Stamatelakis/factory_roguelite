using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UI.QuestBook.Tasks;

namespace UI.QuestBook {
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
        public static string Serialize(QuestBookLibrary library)
        {
            if (library == null) return null;
            List<SerializedQuestBook> serializedBooks = new List<SerializedQuestBook>();
            foreach (QuestBook questBook in library.QuestBooks) {
                serializedBooks.Add(ConvertQuestBook(questBook));
            }
            SerializedQuestBookLibrary serializedQuestBookLibrary = new SerializedQuestBookLibrary(serializedBooks);
            return JsonConvert.SerializeObject(serializedQuestBookLibrary);
        }
        
        private static SerializedQuestBook ConvertQuestBook(QuestBook questBook) {
            List<SerializedQuestBookPage> pages = new List<SerializedQuestBookPage>();
            foreach (QuestBookPage page in questBook.Pages) {
                pages.Add(ConvertQuestBookPage(page));
            }
            return new SerializedQuestBook(
                pages,
                questBook.Title,
                questBook.SpritePath
            );
        }

        private static SerializedQuestBookPage ConvertQuestBookPage(QuestBookPage page) {
            List<SerializedQuestBookNode> nodes = new List<SerializedQuestBookNode>();
            foreach (QuestBookNode node in page.Nodes) {
                nodes.Add(ConvertQuestBookNode(node));
            }
            return new SerializedQuestBookPage(
                page.Title,
                nodes
            );
        }

        private static SerializedQuestBookNode ConvertQuestBookNode(QuestBookNode questBookNode) {
            return new SerializedQuestBookNode(
                questBookNode.NodeData,
                SerializeTask(questBookNode.Content.Task)
            );
        }

        

        public static QuestBookLibrary Deserialize(string json) {
            SerializedQuestBookLibrary serializedQuestBookLibrary = JsonConvert.DeserializeObject<SerializedQuestBookLibrary>(json);
            List<QuestBook> questBooks = new List<QuestBook>();
            foreach (SerializedQuestBook serializedQuestBook in serializedQuestBookLibrary.books) {
                questBooks.Add(DeserializeBook(serializedQuestBook));
            }
            return new QuestBookLibrary(
                questBooks
            );
        }

        private static QuestBook DeserializeBook(SerializedQuestBook serializedQuestBook) {
            List<QuestBookPage> pages = new List<QuestBookPage>();
            foreach (SerializedQuestBookPage page in serializedQuestBook.pages) {
                pages.Add(DeserializePage(page));
            }
            return new QuestBook(
                pages,
                serializedQuestBook.title,
                serializedQuestBook.spritePath
            );
        }

        private static QuestBookPage DeserializePage(SerializedQuestBookPage page) {
            List<QuestBookNode> nodes = new List<QuestBookNode>();
            foreach (SerializedQuestBookNode node in page.nodes) {
                nodes.Add(DeserializeNode(node));
            }
            return new QuestBookPage(
                page.title,
                nodes
            );
        }

        private static QuestBookNode DeserializeNode(SerializedQuestBookNode node)
        {
            QuestBookNodeData nodeData = node.QuestBookNodeData;
            nodeData.Content.Task = DeserializeTask(node.SerializedQuestBookTaskData);
            return new QuestBookNode(
                nodeData,
                null
            );
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


        private class SerializedQuestBookLibrary {
            public List<SerializedQuestBook> books;
            public SerializedQuestBookLibrary(List<SerializedQuestBook> books) {
                this.books = books;
            }
        }

        private class SerializedQuestBook {
            public List<SerializedQuestBookPage> pages;
            public string title;
            public string spritePath;
            public SerializedQuestBook(List<SerializedQuestBookPage> pages, string title, string spritePath) {
                this.title = title;
                this.pages = pages;
                this.spritePath = spritePath;
            }
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
    
    

}


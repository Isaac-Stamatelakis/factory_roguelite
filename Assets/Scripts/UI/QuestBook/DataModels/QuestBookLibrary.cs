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
                questBookNode.X,
                questBookNode.Y,
                JsonConvert.SerializeObject(questBookNode.ImageSeralizedItemSlot),
                ConvertQuestBookNodeContent(questBookNode.Content),
                questBookNode.Prerequisites,
                questBookNode.Id,
                questBookNode.RequireAllPrerequisites
            );
        }

        private static SerializedQuestBookContent ConvertQuestBookNodeContent(QuestBookNodeContent content) {
            string seralizedTask = QuestTaskFactory.serialize(content.Task);
            return new SerializedQuestBookContent(
                seralizedTask,
                content.Description,
                content.Title,
                content.ItemRewards,
                content.CommandRewards,
                content.Size
            );
        }

        public static QuestBookLibrary Deseralize(string json) {
            SerializedQuestBookLibrary serializedQuestBookLibrary = JsonConvert.DeserializeObject<SerializedQuestBookLibrary>(json);
            List<QuestBook> questBooks = new List<QuestBook>();
            foreach (SerializedQuestBook serializedQuestBook in serializedQuestBookLibrary.books) {
                questBooks.Add(DeseralizeBook(serializedQuestBook));
            }
            return new QuestBookLibrary(
                questBooks
            );
        }

        private static QuestBook DeseralizeBook(SerializedQuestBook serializedQuestBook) {
            List<QuestBookPage> pages = new List<QuestBookPage>();
            foreach (SerializedQuestBookPage page in serializedQuestBook.pages) {
                pages.Add(DeseralizePage(page));
            }
            return new QuestBook(
                pages,
                serializedQuestBook.title,
                serializedQuestBook.spritePath
            );
        }

        private static QuestBookPage DeseralizePage(SerializedQuestBookPage page) {
            List<QuestBookNode> nodes = new List<QuestBookNode>();
            foreach (SerializedQuestBookNode node in page.nodes) {
                nodes.Add(DeseralizeNode(node));
            }
            return new QuestBookPage(
                page.title,
                nodes
            );
        }

        private static QuestBookNode DeseralizeNode(SerializedQuestBookNode node) {
            return new QuestBookNode(
                new Vector2(node.x,node.y),
                JsonConvert.DeserializeObject<SerializedItemSlot>(node.serializedItemImage),
                DeseralizeContent(node.content),
                node.connections,
                node.id,
                node.requireAllPrerequisites
            );
        }

        private static QuestBookNodeContent DeseralizeContent(SerializedQuestBookContent content) {
            QuestBookTask task = QuestTaskFactory.deseralize(content.task);
            return new QuestBookNodeContent(
                task,
                content.description,
                content.title,
                content.ItemRewards,
                content.CommandRewards,
                content.Size
            );
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

        private class SerializedQuestBookNode {
            public HashSet<int> connections;
            public string serializedItemImage;
            public float x;
            public float y;
            public int id;
            public SerializedQuestBookContent content;
            public bool requireAllPrerequisites;

            public SerializedQuestBookNode(float x, float y, string serializedItemImage, SerializedQuestBookContent content, HashSet<int> connections, int id, bool requireAllPrerequisites) {
                this.x = x;
                this.y = y;
                this.serializedItemImage = serializedItemImage;
                this.connections = connections;
                this.content = content;
                this.id = id;
                this.requireAllPrerequisites = requireAllPrerequisites;
            }
        }

        private class SerializedQuestBookContent {
            public string task;
            public string description;
            public string title;
            public int numberOfRewards;
            public QuestBookItemRewards ItemRewards;
            public QuestBookCommandRewards CommandRewards;
            public QuestBookNodeSize Size;
            public SerializedQuestBookContent(string task, string description, string title,
                QuestBookItemRewards itemRewards, QuestBookCommandRewards commandRewards, QuestBookNodeSize size) {
                this.task = task;
                this.description = description;
                this.title = title;
                this.ItemRewards = itemRewards;
                this.CommandRewards = commandRewards;
                this.Size = size;
            }
        }
    
    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    /// <summary>
    /// A collection of quest books
    /// </summary>
    public class QuestBookLibrary
    {
        private List<QuestBook> questBooks;

        public List<QuestBook> QuestBooks { get => questBooks; set => questBooks = value; }
        [JsonIgnore] public Dictionary<int, QuestBookNode> IdNodeMap { get => idNodeMap;}

        public QuestBookLibrary(List<QuestBook> books) {
            this.questBooks = books;
            initIdNodeMap();
        }
        private void initIdNodeMap() {
            idNodeMap = new Dictionary<int, QuestBookNode>();
            foreach (QuestBook questBook in questBooks) {
                foreach (QuestBookPage page in questBook.Pages) {
                    foreach (QuestBookNode node in page.Nodes) {
                        if (idNodeMap.ContainsKey(node.Id)) {
                            Debug.LogWarning("Nodes " + node.Content.Title + " and " + idNodeMap[node.Id].Content.Title+ " have duplicate id:" + node.Id);
                            continue;
                        }
                        idNodeMap[node.Id] = node;
                    }
                }
            }
        }

        public int getSmallestNewID() {
            int smallestNewID = 0;
            while (idNodeMap.ContainsKey(smallestNewID)) {
                smallestNewID++;
            }
            return smallestNewID;
        }

        public void addNode(QuestBookNode node) {
            idNodeMap[node.Id] = node;
        }

        public QuestBookNode getNode(int id) {
            if (idNodeMap.ContainsKey(id)) {
                return idNodeMap[id];
            }
            return null;
        }

        public void removeNode(QuestBookNode node) {
            if (idNodeMap.ContainsKey(node.Id)) {
                idNodeMap.Remove(node.Id);
            }
        }

        
        [JsonIgnore] private Dictionary<int, QuestBookNode> idNodeMap;
    }

    public static class QuestBookLibraryFactory {
        public static string seralize(QuestBookLibrary library) {
            List<SerializedQuestBook> serializedBooks = new List<SerializedQuestBook>();
            foreach (QuestBook questBook in library.QuestBooks) {
                serializedBooks.Add(convertQuestBook(questBook));
            }
            SerializedQuestBookLibrary serializedQuestBookLibrary = new SerializedQuestBookLibrary(serializedBooks);
            return JsonConvert.SerializeObject(serializedQuestBookLibrary);
        }
        private static SerializedQuestBook convertQuestBook(QuestBook questBook) {
            List<SerializedQuestBookPage> pages = new List<SerializedQuestBookPage>();
            foreach (QuestBookPage page in questBook.Pages) {
                pages.Add(convertQuestBookPage(page));
            }
            return new SerializedQuestBook(
                pages,
                questBook.Title,
                questBook.SpriteKey
            );
        }

        private static SerializedQuestBookPage convertQuestBookPage(QuestBookPage page) {
            List<SerializedQuestBookNode> nodes = new List<SerializedQuestBookNode>();
            foreach (QuestBookNode node in page.Nodes) {
                nodes.Add(convertQuestBookNode(node));
            }
            return new SerializedQuestBookPage(
                page.Title,
                nodes
            );
        }

        private static SerializedQuestBookNode convertQuestBookNode(QuestBookNode questBookNode) {
            return new SerializedQuestBookNode(
                questBookNode.X,
                questBookNode.Y,
                questBookNode.ImageSeralizedItemSlot,
                convertQuestBookNodeContent(questBookNode.Content),
                questBookNode.Prerequisites,
                questBookNode.Id,
                questBookNode.RequireAllPrerequisites
            );
        }

        private static SerializedQuestBookContent convertQuestBookNodeContent(QuestBookNodeContent content) {
            string seralizedTask = QuestTaskFactory.serialize(content.Task);
            return new SerializedQuestBookContent(
                seralizedTask,
                content.Description,
                content.Title,
                JsonConvert.SerializeObject(content.Rewards),
                content.NumberOfRewards
            );
        }

        public static QuestBookLibrary deseralize(string json) {
            SerializedQuestBookLibrary serializedQuestBookLibrary = JsonConvert.DeserializeObject<SerializedQuestBookLibrary>(json);
            List<QuestBook> questBooks = new List<QuestBook>();
            foreach (SerializedQuestBook serializedQuestBook in serializedQuestBookLibrary.books) {
                questBooks.Add(deseralizeBook(serializedQuestBook));
            }
            return new QuestBookLibrary(
                questBooks
            );
        }

        private static QuestBook deseralizeBook(SerializedQuestBook serializedQuestBook) {
            List<QuestBookPage> pages = new List<QuestBookPage>();
            foreach (SerializedQuestBookPage page in serializedQuestBook.pages) {
                pages.Add(deseralizePage(page));
            }
            return new QuestBook(
                pages,
                serializedQuestBook.title,
                serializedQuestBook.spritePath
            );
        }

        private static QuestBookPage deseralizePage(SerializedQuestBookPage page) {
            List<QuestBookNode> nodes = new List<QuestBookNode>();
            foreach (SerializedQuestBookNode node in page.nodes) {
                nodes.Add(deseralizeNode(node));
            }
            return new QuestBookPage(
                page.title,
                nodes
            );
        }

        private static QuestBookNode deseralizeNode(SerializedQuestBookNode node) {
            return new QuestBookNode(
                new Vector2(node.x,node.y), 
                node.itemImageID,
                deseralizeContent(node.content),
                node.connections,
                node.id,
                node.requireAllPrerequisites
            );
        }

        private static QuestBookNodeContent deseralizeContent(SerializedQuestBookContent content) {
            QuestBookTask task = QuestTaskFactory.deseralize(content.task);
            return new QuestBookNodeContent(
                task,
                content.description,
                content.title,
                JsonConvert.DeserializeObject<List<SerializedItemSlot>>(content.rewards),
                content.numberOfRewards
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
            public string itemImageID;
            public float x;
            public float y;
            public int id;
            public SerializedQuestBookContent content;
            public bool requireAllPrerequisites;

            public SerializedQuestBookNode(float x, float y, string itemImageID, SerializedQuestBookContent content, HashSet<int> connections, int id, bool requireAllPrerequisites) {
                this.x = x;
                this.y = y;
                this.itemImageID = itemImageID;
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
            public string rewards;
            public SerializedQuestBookContent(string task, string description, string title, string rewards, int numberOfRewards) {
                this.task = task;
                this.description = description;
                this.title = title;
                this.rewards = rewards;
                this.numberOfRewards = numberOfRewards;
            }
        }
    }
}


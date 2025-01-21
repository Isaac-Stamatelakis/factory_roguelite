using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.NodeNetwork;

namespace UI.QuestBook {
    public class QuestBookPageUI : NodeNetworkUI<QuestBookNode, QuestBookPage, QuestEditModeController>
    {
        [SerializeField] private QuestBookNodeObject questBookNodeObjectPrefab;
        [SerializeField] private QuestEditModeController questEditModeController;
        [SerializeField] private GameObject linePrefab;
        private QuestBookLibrary library;
        public QuestBookLibrary Library {get => library;}
        private QuestBook questBook;
        public QuestBook QuestBook {get => questBook;}
        private QuestBookUI questBookUI;
        public QuestBookUI QuestBookUI {get => questBookUI;}
        public void Initialize(QuestBookPage questBookPage, QuestBook questBook, QuestBookLibrary questBookLibrary, QuestBookUI questBookUI)
        {
            CurrentSelected = null;
            this.NodeNetwork = questBookPage;
            this.questBook = questBook;
            this.library = questBookLibrary;
            this.questBookUI = questBookUI;
            if (QuestBookUtils.EditMode) {
                initEditMode();
            }
        }
        public override void DisplayLines()
        {
            GlobalHelper.deleteAllChildren(LineContainer);
            HashSet<int> pageIds = new HashSet<int>();
            foreach (QuestBookNode node in NodeNetwork.getNodes()) {
                pageIds.Add(node.Id);
            }
            Dictionary<int, QuestBookNode> idNodeMap = library.IdNodeMap;
            foreach (QuestBookNode questBookNode in nodeNetwork.getNodes()) {
                foreach (int id in questBookNode.Prerequisites) {
                    if (!pageIds.Contains(id)) {
                        continue;
                    }
                    QuestBookNode otherNode = library.IdNodeMap[id];
                    bool discovered = nodeDiscovered(questBookNode);
                    QuestBookUIFactory.GenerateLine(otherNode.getPosition(),questBookNode.getPosition(),LineContainer,discovered,linePrefab);
                }
            }
        }
        protected override INodeUI GenerateNode(QuestBookNode node)
        {
            QuestBookNodeObject nodeObject = GameObject.Instantiate(questBookNodeObjectPrefab);
            nodeObject.Init(node,this);
            nodeObject.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return nodeObject;
        }

        protected override void initEditMode()
        {
            questEditModeController.gameObject.SetActive(true);
            questEditModeController.init(this);
        }

        protected override bool nodeDiscovered(QuestBookNode node)
        {
            
            foreach (int prereqID in node.Prerequisites) {
                if (!library.IdNodeMap.ContainsKey(prereqID)) continue;
                bool preReqComplete = library.IdNodeMap[prereqID].Content.Task.IsComplete();
                if (node.RequireAllPrerequisites && !preReqComplete)  {
                    return false;
                }
                if (!node.RequireAllPrerequisites && preReqComplete) {
                    return true;
                }
            }
            // If the loop has gotten to this point, there are two cases
            // i) If its RequireAllPrequestites, then all are complete so return RequireAllPrequresites aka true
            // ii) If its not RequireAllPrequesites, then atleast one is not complete so return not RequireAllPrequreistes aka false
            return node.RequireAllPrerequisites;
        }

        public override void AddNode(INodeUI nodeUI)
        {
            
        }

        public override void PlaceNewNode(Vector2 position)
        {
            SerializedItemSlot slot = new SerializedItemSlot("stone", 1, null);
            QuestBookNode node = new QuestBookNode(
                position,
                slot,
                new QuestBookNodeContent(
                    new CheckMarkQuestTask(),
                    "Empty Description",
                    "New Task",
                    new QuestBookItemRewards(new List<SerializedItemSlot>(), int.MaxValue),
                    new QuestBookCommandRewards(new List<QuestBookCommandReward>())
                ),
                new HashSet<int>(),
                library.GetSmallestNewID(),
                true
            );
            nodeNetwork.Nodes.Add(node);
            library.AddNode(node);
        }

        public override GameObject GenerateNewNodeObject()
        {
            return GameObject.Instantiate(questBookNodeObjectPrefab).gameObject;
        }
    }
}


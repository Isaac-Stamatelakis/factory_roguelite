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
        public void init(QuestBookPage questBookPage, QuestBook questBook, QuestBookLibrary questBookLibrary, QuestBookUI questBookUI) {
            this.NodeNetwork = questBookPage;
            this.questBook = questBook;
            this.library = questBookLibrary;
            this.questBookUI = questBookUI;
            if (QuestBookHelper.EditMode) {
                initEditMode();
            }
        }
        public override void displayLines()
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
                    QuestBookUIFactory.generateLine(questBookNode.Position,otherNode.Position,LineContainer,discovered,linePrefab);
                }
            }
        }
        protected override void generateNode(QuestBookNode node)
        {
            QuestBookNodeObject nodeObject = GameObject.Instantiate(questBookNodeObjectPrefab);
            nodeObject.init(node,this);
            nodeObject.transform.SetParent(nodeContainer,false);
        }

        protected override void initEditMode()
        {
            questEditModeController.gameObject.SetActive(true);
            questEditModeController.init(this);
        }

        protected override bool nodeDiscovered(QuestBookNode node)
        {
            
            foreach (int prereqID in node.Prerequisites) {
                bool preReqComplete = library.IdNodeMap[prereqID].Content.Task.getComplete();
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

        public override void addNode(INodeUI nodeUI)
        {
            throw new System.NotImplementedException();
        }

        public override void placeNewNode(Vector2 position)
        {
            QuestBookNode node = new QuestBookNode(
                    position,
                    null,
                    new QuestBookNodeContent(
                        new CheckMarkQuestTask(),
                        "Empty Description",
                        "New Task",
                        new List<SerializedItemSlot>(),
                        9999
                    ),
                    new HashSet<int>(),
                    library.getSmallestNewID(),
                    true
                );
                nodeNetwork.Nodes.Add(node);
                library.addNode(node);
        }

        public override GameObject generateNewNodeObject()
        {
            return GameObject.Instantiate(questBookNodeObjectPrefab).gameObject;
        }
    }
}


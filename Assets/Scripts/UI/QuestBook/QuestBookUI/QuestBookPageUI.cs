using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.NodeNetwork;
using UI.QuestBook.Tasks;

namespace UI.QuestBook {
    public class QuestBookPageUI : NodeNetworkUI<QuestBookNode, QuestBookPage>
    {
        [SerializeField] private QuestBookNodeObject questBookNodeObjectPrefab;
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
            editController.gameObject.SetActive(QuestBookUtils.EditMode);
        }
        
        protected override INodeUI GenerateNode(QuestBookNode node)
        {
            QuestBookNodeObject nodeObject = GameObject.Instantiate(questBookNodeObjectPrefab);
            nodeObject.Initialize(node,this);
            RectTransform nodeRectTransform = (RectTransform)nodeObject.transform;
            nodeRectTransform.sizeDelta = GetNodeVectorSize(node.NodeData.Size);
            nodeObject.transform.SetParent(nodeContainer,false); // Even though rider suggests changing this, it is wrong to
            return nodeObject;
        }

        public override bool ShowAllComplete()
        {
            return QuestBookUtils.SHOW_ALL_COMPLETED;
        }

        public override void OnDeleteSelectedNode()
        {
            // Does nothing
        }

        public override QuestBookNode LookUpNode(int id)
        {
            return library.IdNodeMap.GetValueOrDefault(id);
        }
        

        private Vector2 GetNodeVectorSize(QuestBookNodeSize nodeSize)
        {
            const float BASE_SIZE = 64f;
            const float LARGE_SIZE_MULTIPLIER = 1.5f;
            const float HUGE_SIZE_MULTIPLIER = 2f;
            switch (nodeSize)
            {
                case QuestBookNodeSize.Regular:
                    return new Vector2(BASE_SIZE, BASE_SIZE);
                case QuestBookNodeSize.Large:
                    return new Vector2(BASE_SIZE, BASE_SIZE)*LARGE_SIZE_MULTIPLIER;
                case QuestBookNodeSize.Huge:
                    return new Vector2(BASE_SIZE, BASE_SIZE)*HUGE_SIZE_MULTIPLIER;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeSize), nodeSize, null);
            }
        }
        

        public override void PlaceNewNode(Vector2 position)
        {
            SerializedItemSlot defaultItemImage = new SerializedItemSlot("stone", 1, null);
            QuestBookNodeContent defaultContent = new QuestBookNodeContent(
                new CheckMarkQuestTask(),
                "Empty Description",
                "New Task",
                new QuestBookItemRewards(new List<SerializedItemSlot>(), false),
                new QuestBookCommandRewards(new List<QuestBookCommandReward>())
            );
            QuestBookNodeData defaultNodeData = new QuestBookNodeData(
                new List<int>(),
                defaultItemImage,
                position.x,
                position.y,
                true,
                library.GetSmallestNewID(),
                QuestBookNodeSize.Regular,
                defaultContent
            );
            QuestBookNode node = new QuestBookNode(
                defaultNodeData,
                null
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


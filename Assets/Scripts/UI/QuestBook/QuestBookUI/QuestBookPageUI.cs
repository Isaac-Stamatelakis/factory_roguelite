using System;
using System.Collections;
using System.Collections.Generic;
using DevTools;
using UnityEngine;
using UI.NodeNetwork;
using UI.QuestBook.Data;
using UI.QuestBook.Data.Node;
using UI.QuestBook.Data.Rewards;
using UI.QuestBook.Tasks;

namespace UI.QuestBook {
    public class QuestBookPageUI : NodeNetworkUI<QuestBookNode, QuestBookPage>
    {
        [SerializeField] private QuestBookNodeObject questBookNodeObjectPrefab;
        private QuestBookUI questBookUI;
        public QuestBookUI QuestBookUI {get => questBookUI;}
        private QuestBookData questBookData;
        private QuestBookPageData questBookPageData;
        private string questBookPath;
        public string QuestBookPath {get => questBookPath;}
        private string playerDataPath;

        public void Initialize(QuestBookPage questBookPage, QuestBookData questBookData, QuestBookPageData questBookPageData, QuestBookUI questBookUI, string questBookPath, string playerDataPath)
        {
            if (this.nodeNetwork != null)
            {
                if (DevToolUtils.OnDevToolScene)
                {
                    SavePageData();
                }
                else
                {
                    SavePlayerData();
                }
            }
            CurrentSelected = null;
            this.questBookUI = questBookUI;
            this.NodeNetwork = questBookPage;
            this.questBookData = questBookData;
            this.questBookUI = questBookUI;
            this.questBookPageData = questBookPageData;
            this.questBookPath = questBookPath;
            bool editMode = DevToolUtils.OnDevToolScene;
            editController.gameObject.SetActive(editMode);
            this.playerDataPath = playerDataPath;
            if (editMode)
            {
                editController.Initialize(this);
            }
            
            Display();
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
            return NodeNetwork.IdNodeDict.GetValueOrDefault(id);
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
                questBookData.IDCounter,
                QuestBookNodeSize.Regular,
                defaultContent
            );
            QuestBookNode node = new QuestBookNode(
                defaultNodeData,
                new QuestBookTaskData(false,new QuestBookRewardClaimStatus(),defaultNodeData.Id)
            );
            questBookData.IDCounter++;
            nodeNetwork.Nodes.Add(node);
            nodeNetwork.IdNodeDict[node.Id] = node;
        }

        public override GameObject GenerateNewNodeObject()
        {
            return GameObject.Instantiate(questBookNodeObjectPrefab).gameObject;
        }

        public void OnDestroy()
        {
            if (DevToolUtils.OnDevToolScene)
            {
                SavePageData();
            }
        }

        private void SavePageData()
        {
            if (nodeNetwork == null) return;
            List<QuestBookNodeData> questBookNodeDataList = new List<QuestBookNodeData>();
            foreach (QuestBookNode node in NodeNetwork.Nodes)
            {
                questBookNodeDataList.Add(node.NodeData);
            }
            QuestBookFactory.SerializedQuestBookNodeData(questBookPath,questBookPageData.Id,questBookNodeDataList);
        }

        private void SavePlayerData()
        {
            if (nodeNetwork == null) return;
            List<QuestBookTaskData> questBookNodeDataList = new List<QuestBookTaskData>();
            foreach (QuestBookNode node in NodeNetwork.Nodes)
            {
                questBookNodeDataList.Add(node.TaskData);
            }
            GlobalHelper.SerializeCompressedJson(questBookNodeDataList,playerDataPath);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Newtonsoft.Json;
using UI.NodeNetwork;
using UI.QuestBook.Tasks;

namespace UI.QuestBook {
    public class QuestBookNodeData
    {
        public List<int> Prerequisites;
        public SerializedItemSlot ImageSeralizedItemSlot;
        public float X;
        public float Y;
        public bool RequireAllPrerequisites;
        public int Id;
        public QuestBookNodeContent Content;
        public QuestBookNodeSize Size;

        public QuestBookNodeData(List<int> prerequisites, SerializedItemSlot imageSeralizedItemSlot, float x, float y, bool requireAllPrerequisites, int id, QuestBookNodeSize size, QuestBookNodeContent content)
        {
            Prerequisites = prerequisites;
            ImageSeralizedItemSlot = imageSeralizedItemSlot;
            X = x;
            Y = y;
            RequireAllPrerequisites = requireAllPrerequisites;
            Id = id;
            Content = content;
        }
    }

    public interface ICompletionCheckQuest
    {
        public bool CheckCompletion();
    }
    
    public class QuestBookTaskData
    {
        public bool Complete;
        public int Id;
        public QuestBookRewardClaimStatus RewardStatus;
        public QuestBookTaskData(bool complete, QuestBookRewardClaimStatus rewardStatus, int id)
        {
            Complete = complete;
            RewardStatus = rewardStatus;
            Id = id;
        }
    }

    

    public class QuestBookRewardClaimStatus
    {
        public bool ItemsClaimed;
        public bool CommandsClaimed;
    }
    public class QuestBookNode : INode
    {
        public QuestBookNodeData NodeData;
        public QuestBookTaskData TaskData;

        [JsonIgnore] public QuestBookNodeContent Content => NodeData.Content;
        [JsonIgnore] public int Id => NodeData.Id;
        
        public QuestBookNode(QuestBookNodeData nodeData, QuestBookTaskData taskData) {
            this.NodeData = nodeData;
            this.TaskData = taskData;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(NodeData.X,NodeData.Y,0);
        }

        public void SetPosition(Vector3 pos)
        {
            NodeData.X = pos.x;
            NodeData.Y = pos.y;
        }

        public int GetId()
        {
            return NodeData.Id;
        }

        public List<int> GetPrerequisites()
        {
            return NodeData.Prerequisites;
        }

        public bool IsCompleted()
        {
            return TaskData?.Complete ?? false;
        }
    }
}


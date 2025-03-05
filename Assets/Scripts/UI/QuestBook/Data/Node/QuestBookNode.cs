using System.Collections.Generic;
using Newtonsoft.Json;
using UI.NodeNetwork;
using UnityEngine;

namespace UI.QuestBook.Data.Node {
    public class QuestBookNode : INode
    {
        public QuestBookNodeData NodeData;
        public QuestBookTaskData TaskData;
        public QuestBookNodeContent Content => NodeData.Content;
        public int Id => NodeData.Id;
        
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


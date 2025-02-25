using System.Collections.Generic;
using UI.NodeNetwork;
using UnityEngine;

namespace Robot.Upgrades.Network
{
    public class RobotUpgradeNode : INode
    {
        public RobotUpgradeNodeData NodeData;
        public RobotUpgradeData InstanceData;
        public Vector3 GetPosition()
        {
            return new Vector3(NodeData.X, NodeData.Y);
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
            return NodeData.PreReqs;
        }

        public bool IsCompleted()
        {
            return (InstanceData?.Amount ?? 0) >= NodeData.UpgradeAmount;
        }

        public RobotUpgradeNode(RobotUpgradeNodeData nodeData, RobotUpgradeData instanceData)
        {
            NodeData = nodeData;
            InstanceData = instanceData;
        }
    }
}
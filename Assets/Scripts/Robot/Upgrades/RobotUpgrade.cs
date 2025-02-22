using System.Collections.Generic;
using UI.NodeNetwork;
using UnityEngine;

namespace Robot.Upgrades {
    public enum RobotUpgradeType
    {
        Self,
        Tool
    }

    public class RobotUpgradeData
    {
        public List<RobotUpgradeData> Upgrades;
    }

    public class RobotUpgradeNode : INode
    {
        public int Id;
        public int UpgradeType;
        public int UpgradeAmount = 1;
        public int CostMultiplier = 1;
        public List<SerializedItemSlot> Cost;
        public List<int> PreReqs;
        public Vector2Int Position;
        public string IconItemId;
        public Vector3 GetPosition()
        {
            return new Vector3(Position.x, Position.y, 0);
        }

        public void SetPosition(Vector3 pos)
        {
            Position = new Vector2Int((int)pos.x, (int)pos.y);
        }

        public int GetId()
        {
            return Id;
        }

        public List<int> GetPrerequisites()
        {
            return PreReqs;
        }
    }

    public class RobotUpgradeNodeNetwork : INodeNetwork<RobotUpgradeNode>
    {
        public RobotUpgradeType Type;
        public int SubType;
        public List<RobotUpgradeNode> UpgradeNodes;
        public List<RobotUpgradeNode> getNodes()
        {
            return UpgradeNodes;
        }

        public RobotUpgradeNodeNetwork(RobotUpgradeType type, int subType, List<RobotUpgradeNode> upgradeNodes)
        {
            Type = type;
            SubType = subType;
            UpgradeNodes = upgradeNodes;
        }
    }
    public class RobotUpgradeNodeData
    {
        public int Id;
        public int Amount;

        public RobotUpgradeNodeData(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
    
}


using System.Collections.Generic;
using Newtonsoft.Json;
using UI.NodeNetwork;
using UnityEngine;
using WorldModule;

namespace Robot.Upgrades {
    public static class RobotUpgradeUtils
    {
        public static RobotUpgradeNodeNetwork DeserializeRobotNodeNetwork(byte[] bytes)
        {
            string json = WorldLoadUtils.DecompressString(bytes);
            if (json == null) return null;
            try
            {
                return JsonConvert.DeserializeObject<RobotUpgradeNodeNetwork>(json);
            }
            catch
            {
                return null;
            }
            
        } 
        public static int GetNextUpgradeId(RobotUpgradeNodeNetwork robotUpgradeNodeNetwork)
        {
            List<RobotUpgradeNode> nodes = robotUpgradeNodeNetwork.GetNodes();
            if (nodes == null) return 0;
            int lowest = 0;
            foreach (RobotUpgradeNode node in nodes)
            {
                int id = node.GetId();
                if (id < lowest) lowest = id;
            }

            return lowest + 1;
        }
    }
    
    public enum RobotUpgradeType
    {
        Tool = 0,
        Robot = 1,
        
    }
    

    public class RobotUpgradeNodeData
    {
        public int Id;
        public int UpgradeType;
        public int UpgradeAmount = 1;
        public int CostMultiplier = 1;
        public List<SerializedItemSlot> Cost;
        public List<int> PreReqs;
        public Vector2Int Position;
        public string IconItemId;

        public RobotUpgradeNodeData(int id)
        {
            Id = id;
        }
    }
    public class RobotUpgradeNode : INode
    {
        public RobotUpgradeNodeData NodeData;
        public RobotUpgradeData InstanceData;
        public Vector3 GetPosition()
        {
            Vector2Int position = NodeData.Position;
            return new Vector3(position.x, position.y, 0);
        }

        public void SetPosition(Vector3 pos)
        {
            NodeData.Position = new Vector2Int((int)pos.x, (int)pos.y);
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

    public class RobotUpgradeNodeNetwork : INodeNetwork<RobotUpgradeNode>
    {
        public RobotUpgradeType Type;
        public int SubType;
        public List<RobotUpgradeNode> UpgradeNodes;
        public List<RobotUpgradeNode> GetNodes()
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
    public class RobotUpgradeData
    {
        public int Id;
        public int Amount;

        public RobotUpgradeData(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
    
}


using System.Collections.Generic;

namespace Robot.Upgrades.Network
{
    public class SerializedRobotUpgradeNodeNetwork
    {
        public RobotUpgradeType Type;
        public int SubType;
        public List<RobotUpgradeNodeData> NodeData;

        public SerializedRobotUpgradeNodeNetwork(RobotUpgradeType type, int subType, List<RobotUpgradeNodeData> nodeData)
        {
            Type = type;
            SubType = subType;
            NodeData = nodeData;
        }
    }
}
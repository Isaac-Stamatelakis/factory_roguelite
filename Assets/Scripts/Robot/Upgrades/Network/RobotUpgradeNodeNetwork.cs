using System.Collections.Generic;
using UI.NodeNetwork;

namespace Robot.Upgrades.Network
{
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
}
using System.Collections.Generic;

namespace Robot.Upgrades.Network
{
    public class RobotUpgradeNodeData
    {
        public int Id;
        public int UpgradeType;
        public int UpgradeAmount = 1;
        public float CostMultiplier = 1;
        public List<SerializedItemSlot> Cost = new List<SerializedItemSlot>();
        public List<int> PreReqs = new List<int>();
        public float X;
        public float Y;
        public string IconItemId;

        public RobotUpgradeNodeData(int id)
        {
            Id = id;
        }
    }
}
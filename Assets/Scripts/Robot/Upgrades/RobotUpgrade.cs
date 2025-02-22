using System.Collections.Generic;
using UnityEngine;

namespace Robot.Upgrades {
    public enum RobotUpgradeType
    {
        Self,
        Tool
    }

    public class RobotUpgradeData
    {
        public RobotUpgradeType Type;
        public int SubType;
    }

    public class RobotUpgradeNode
    {
        public int Id;
        public int UpgradeType;
        public int UpgradeAmount = 1;
        public int CostMultiplier = 1;
        public List<SerializedItemSlot> Cost;
        public List<int> PreReqs;
        public Vector2Int Position;
    }
    
}


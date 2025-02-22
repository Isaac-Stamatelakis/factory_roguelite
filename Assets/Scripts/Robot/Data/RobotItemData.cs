using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Player.Tool;
using Robot.Tool;
using Robot.Upgrades;
using UnityEngine;
namespace RobotModule {
    public class RobotItemData
    {
        public float Health;
        public ulong Energy;
        public ItemRobotToolData ToolData;
        public RobotUpgradeData RobotUpgradeData;
        public RobotItemData(ItemRobotToolData toolData, RobotUpgradeData robotUpgradeData, float health, ulong energy) {
            this.ToolData = toolData;
            this.Health = health;
            this.Energy = energy;
            this.RobotUpgradeData = robotUpgradeData;
        }
    }
}


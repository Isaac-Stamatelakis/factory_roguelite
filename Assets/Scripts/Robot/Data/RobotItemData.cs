using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Player.Tool;
using Robot.Tool;
using Robot.Upgrades;
using Robot.Upgrades.Network;
using UnityEngine;
namespace RobotModule {
    public class RobotItemData
    {
        public float Health;
        public ulong Energy;
        public float nanoBotTime;
        public ItemRobotToolData ToolData;
        public List<RobotUpgradeData> RobotUpgrades;
        public RobotItemData(ItemRobotToolData toolData, List<RobotUpgradeData> robotUpgrades, float health, ulong energy, float nanoBotTime) {
            this.ToolData = toolData;
            this.Health = health;
            this.Energy = energy;
            this.RobotUpgrades = robotUpgrades;
            this.nanoBotTime = nanoBotTime;
        }
    }
}


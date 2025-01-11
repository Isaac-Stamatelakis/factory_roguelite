using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Player.Tool;
using Robot.Tool;
using UnityEngine;
namespace RobotModule {
    public class RobotItemData
    {
        public ItemRobotToolData ToolData;
        public RobotItemData(ItemRobotToolData toolData) {
            this.ToolData = toolData;
        }
    }
}


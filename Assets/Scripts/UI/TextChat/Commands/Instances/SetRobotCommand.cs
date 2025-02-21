using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using Item.Slot;
using PlayerModule;
using Items;
using Items.Tags;
using Player.Tool;
using Robot.Tool;
using Robot.Tool.Instances;
using RobotModule;

namespace UI.Chat {
    public class SetRobotCommand : ChatCommand, IAutoFillChatCommand
    {
        public SetRobotCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            string id = parameters[0];
                

            if (id == "happy_mk1")
            {
                PlayerManager.Instance.GetPlayer().PlayerRobot.SetRobot(RobotDataFactory.GetDefaultRobot());
            }
            else
            {
                RobotItem robotItem = ItemRegistry.GetInstance().GetRobotItem(id);
                if (robotItem == null) {
                    chatUI.SendChatMessage("Invalid id");
                    return;
                }
                    
                ItemSlot itemSlot = new ItemSlot(robotItem, 1, null);
                    
                PlayerManager.Instance.GetPlayer().PlayerRobot.SetRobot(itemSlot);
            }
        }

        public List<string> getAutoFill(int paramIndex)
        {
            List<ItemObject> items = ItemRegistry.GetInstance().GetAllItems();
            List<string> ids = new List<string>();
            foreach (ItemObject itemObject in items) {
                if (itemObject is not RobotItem) {
                    continue;
                }
                ids.Add(itemObject.id);
            }
            return ids;
        }

        public override string getDescription()
        {
            return "/setrobot id\nSets player robot to id";
        }
    }
}

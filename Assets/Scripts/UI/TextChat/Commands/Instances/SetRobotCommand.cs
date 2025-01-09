using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using Item.Slot;
using PlayerModule;
using Items;

namespace UI.Chat {
    public class SetRobotCommand : ChatCommand, IAutoFillChatCommand
    {
        public SetRobotCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            try {
                string id = parameters[0];
                RobotItem robotItem = ItemRegistry.GetInstance().GetRobotItem(id);
                if (robotItem == null) {
                    chatUI.sendMessage("Invalid id");
                    return;
                }
                ItemSlot itemSlot = ItemSlotFactory.CreateNewItemSlot(robotItem,1);
                PlayerManager.Instance.GetPlayer().PlayerRobot.setRobot(itemSlot);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
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

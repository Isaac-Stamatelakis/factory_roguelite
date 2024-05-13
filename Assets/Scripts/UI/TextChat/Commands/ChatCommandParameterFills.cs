using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Entities;
using ItemModule;

namespace UI.Chat {
    public static class ChatCommandParameterFillExtension
    {
        public static List<string> getAutoFill(this ChatCommand command, int paramIndex) {
            return command switch
            {
                ChatCommand.help => getHelpStrings(),
                ChatCommand.spawn => getSpawnStrings(),
                ChatCommand.give => getGiveStrings(paramIndex),
                ChatCommand.gamemode => getGameModeString(),
                ChatCommand.setrobot => getRobotStrings(),
                _ => new List<string>()
            };
        }

        private static List<string> getHelpStrings() {
            return ChatCommandUtils.getAllCommandStrings();
        }
        private static List<string> getSpawnStrings() {
            return EntityUtils.getAllIds();
        }
        private static List<string> getGameModeString() {
            return new List<string> {
                "0",
                "1"
            };
        }
        private static List<string> getRobotStrings() {
            List<ItemObject> items = ItemRegistry.getInstance().getAllItems();
            List<string> ids = new List<string>();
            foreach (ItemObject itemObject in items) {
                if (itemObject is not RobotItem) {
                    continue;
                }
                ids.Add(itemObject.id);
            }
            return ids;
        }
        private static List<string> getGiveStrings(int paramIndex) {
            if (paramIndex != 0) {
                return new List<string>();
            }
            List<ItemObject> items = ItemRegistry.getInstance().getAllItems();
            List<string> ids = new List<string>();
            foreach (ItemObject item in items) {
                ids.Add(item.id);
            }
            return ids;
        }
    }
}


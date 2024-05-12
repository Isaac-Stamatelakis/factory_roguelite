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
        public static List<string> getAutoFill(this ChatCommand command, string paramPrefix, int paramIndex) {
            return command switch
            {
                ChatCommand.help => getHelpStrings(paramPrefix),
                ChatCommand.spawn => getSpawnStrings(paramPrefix),
                ChatCommand.give => getGiveStrings(paramPrefix,paramIndex),
                ChatCommand.gamemode => getGameModeString(),
                ChatCommand.setrobot => getRobotStrings(paramPrefix),
                _ => new List<string>()
            };
        }

        private static List<string> getHelpStrings(string paramPrefix) {
            List<string> strings = ChatCommandUtils.getAllCommandStrings();
            var matchingStrings = strings.Where(s => s.StartsWith(paramPrefix)).ToList();
            return matchingStrings;
        }
        private static List<string> getSpawnStrings(string paramPrefix) {
            return EntityUtils.getAllIds();
        }
        private static List<string> getGameModeString() {
            return new List<string> {
                "0",
                "1"
            };
        }
        private static List<string> getRobotStrings(string paramPrefix) {
            List<ItemObject> items = ItemRegistry.getInstance().getAllItemsWithPrefix(paramPrefix);
            List<string> ids = new List<string>();
            foreach (ItemObject itemObject in items) {
                if (itemObject is not RobotItem) {
                    continue;
                }
                ids.Add(itemObject.id);
            }
            return ids;
        }
        private static List<string> getGiveStrings(string paramPrefix, int paramIndex) {
            if (paramIndex != 0) {
                return new List<string>();
            }
            List<ItemObject> items = ItemRegistry.getInstance().getAllItemsWithPrefix(paramPrefix);
            List<string> ids = new List<string>();
            foreach (ItemObject item in items) {
                ids.Add(item.id);
            }
            return ids;
        }
    }
}


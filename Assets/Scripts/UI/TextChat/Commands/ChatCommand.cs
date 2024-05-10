using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UI;
using PlayerModule;
using ItemModule;

namespace UI.Chat {

    public enum ChatCommand {
        help,
        teleport,
        spawn,
        give,
        setrobot,
        gamemode
    }

    public static class ChatCommandUtils {
        private static readonly int commandsPerHelpPage = 10;

        public static int CommandsPerHelpPage => commandsPerHelpPage;
        public static int getHelpPages() {
            var commands = Enum.GetValues(typeof(ChatCommand));
            return Mathf.CeilToInt(commands.Length/(float) ChatCommandUtils.CommandsPerHelpPage);
        }
        public static List<string> getAllCommandStrings() {
            var commands = Enum.GetValues(typeof(ChatCommand));
            List<string> commandStrings = new List<string>();
            foreach (ChatCommand command in commands) {
                commandStrings.Add(command.ToString());
            }
            return commandStrings;
        }
    }

    public static class ChatCommandFactory {
        public static ChatCommand? getCommand(string text) {
            var commands = Enum.GetValues(typeof(ChatCommand));
            foreach (ChatCommand command in commands) {
                if (text.ToString().Equals(command.ToString())) {
                    return command;
                }
            }
            return null;
        }
    }
}


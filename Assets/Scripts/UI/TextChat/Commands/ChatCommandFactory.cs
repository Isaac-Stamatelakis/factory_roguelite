using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UI;
using PlayerModule;
using Items;

namespace UI.Chat {
    public static class ChatCommandFactory {
        private static readonly Dictionary<string, Func<string[], TextChatUI, ChatCommand>> commandMap = 
            new Dictionary<string, Func<string[], TextChatUI, ChatCommand>>
        {
            { "help", (parameters, ui) => new HelpCommand(parameters, ui) },
            { "setdim", (parameters, ui) => new SetDimCommand(parameters, ui) },
            { "setlight", (parameters, ui) => new SetLightCommand(parameters, ui) },
            { "setoutline", (parameters, ui) => new ModifyOutlineCommand(parameters, ui) },
            { "give", (parameters, ui) => new GiveCommand(parameters, ui) },
            { "spawn", (parameters, ui) => new SpawnCommand(parameters, ui) },
            { "tp", (parameters, ui) => new TeleportCommand(parameters, ui) },
            { "setrobot", (parameters, ui) => new SetRobotCommand(parameters, ui) },
            
        };

        public static List<string> getAllCommands() {
            return new List<string>(commandMap.Keys);
        }
        public static ChatCommand getCommand(ChatCommandToken token, TextChatUI textChatUI) {
            if (commandMap.TryGetValue(token.Command, out var commandConstructor))
            {
                return commandConstructor(token.Parameters, textChatUI);
            }
            return null;
        }
    }
}

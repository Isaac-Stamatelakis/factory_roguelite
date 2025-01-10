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
            { "light", (parameters, ui) => new SetLightCommand(parameters, ui) },
            { "outline", (parameters, ui) => new ModifyOutlineCommand(parameters, ui) },
            { "give", (parameters, ui) => new GiveCommand(parameters, ui) },
            { "spawn", (parameters, ui) => new SpawnCommand(parameters, ui) },
            { "tp", (parameters, ui) => new TeleportCommand(parameters, ui) },
            { "setrobot", (parameters, ui) => new SetRobotCommand(parameters, ui) },
            { "camera", (parameters, ui) => new CameraViewCommand(parameters, ui) },
            { "flight", (parameters, ui) => new FlightCommand(parameters, ui) },
            
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

        public static ChatCommand getEmptyCommand(string command, TextChatUI textChatUI) {
            if (commandMap.TryGetValue(command, out var commandConstructor))
            {
                return commandConstructor(null, textChatUI);
            }
            return null;
        }
    }
}


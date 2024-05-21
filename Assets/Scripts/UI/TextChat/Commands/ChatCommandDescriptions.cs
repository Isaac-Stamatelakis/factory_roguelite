using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace UI.Chat {

    public static class ChatCommandDescriptionExtension {
        public static string getDescription(this ChatCommand command) {
            return command switch
            {
                ChatCommand.help => getHelpDescription(),
                ChatCommand.teleport => "/teleport x y\nTeleports the player to the specified coordinates (x, y) within the current dimension.",
                ChatCommand.spawn => "/spawn id\nSpawns entity with id at player location",
                ChatCommand.give => "/give id amount\nGives player amount of item with id",
                ChatCommand.gamemode => "/gamemode x\n0: Surival mode\n1:Creative mode",
                ChatCommand.setrobot => "/setrobot id\nSets player robot to id",
                ChatCommand.setlight => "/lightmode intensity r g b \nSets light to intensity in range [0,inf)\nOptionally provide r,g,b in range [0,255]",
                ChatCommand.setdim => "/setdim dim position\nTeleports player to dimension at position",
                _ => ""
            };
        }

        private static string getHelpDescription() {
            var commands = Enum.GetValues(typeof(ChatCommand));
            int pages = Mathf.CeilToInt(commands.Length/ChatCommandUtils.CommandsPerHelpPage);
            string output = $"Displays a list of all available commands. Optional parameter: specify a page number between 1 and {pages+1} to view commands in segments. Enter a command to receive a detailed description.";
            return output;
        }
    }
}


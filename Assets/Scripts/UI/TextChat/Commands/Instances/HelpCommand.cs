using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UI.Chat {
    public class HelpCommand : ChatCommand, IAutoFillChatCommand
    {
        private static readonly int commandsPerHelpPage = 10;
        public HelpCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            ChatCommand chatCommand = ChatCommandFactory.getEmptyCommand(parameters[0],chatUI);
            if (chatCommand == null)
            {
                throw new ChatParseException($"Error running 'help': {parameters[0]} is not a valid command");
            }
            chatUI.sendMessage($"Usage of {parameters[0]}:\n{chatCommand.getDescription()}");

        }

        public List<string> getAutoFill(int paramIndex)
        {
            return ChatCommandFactory.getAllCommands();
        }
        
        public int getPages() {
            List<string> commands = ChatCommandFactory.getAllCommands();
            return Mathf.CeilToInt(commands.Count/(float) commandsPerHelpPage);
        }

        public override string getDescription()
        {
            List<string> commands = ChatCommandFactory.getAllCommands();
            int pages = Mathf.CeilToInt(commands.Count/commandsPerHelpPage);
            string output = $"Displays a list of all available commands. Optional parameter: specify a page number between 1 and {pages+1} to view commands in segments. Enter a command to receive a detailed description.";
            return output;
        }
    }
}

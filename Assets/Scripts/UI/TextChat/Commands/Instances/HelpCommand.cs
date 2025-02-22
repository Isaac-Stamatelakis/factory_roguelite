using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UI.Chat {
    public class HelpCommand : ChatCommand, IAutoFillChatCommand
    {
        private const int COMMANDS_PER_PAGE = 10;
        public HelpCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            if (parameters.Length == 0)
            { 
                List<string> commands = ChatCommandFactory.getAllCommands();
                SendCommandListMessage(commands, prefix: $"Commands: ");
                return;
            }
            if (TryParsePageNumber()) return;
            ChatCommand chatCommand = ChatCommandFactory.getEmptyCommand(parameters[0],chatUI);
            if (chatCommand == null)
            {
                throw new ChatParseException($"Error running 'help': {parameters[0]} is not a valid command");
            }
            chatUI.SendChatMessage($"Usage of {parameters[0]}:\n{chatCommand.getDescription()}");

        }

        private bool TryParsePageNumber()
        {
            try
            {
                int result = Convert.ToInt32(parameters[0])-1;
                List<string> commands = ChatCommandFactory.getAllCommands();
                if (result < 0 || result * COMMANDS_PER_PAGE >= commands.Count)
                {
                    throw new ChatParseException($"Error running 'help': page {parameters[0]} is out of range");
                }

                List<string> commandsToSend = new List<string>();
                for (int i = 0; i < COMMANDS_PER_PAGE; i++)
                {
                    int index = COMMANDS_PER_PAGE * result + i;
                    if (index >= commands.Count) break;
                    commandsToSend.Add(commands[index]);
                }
                
                SendCommandListMessage(commandsToSend, prefix: $"Page {result+1}: ");
                
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void SendCommandListMessage(List<string> commandsToSend, string prefix = null)
        {
            string chat = string.Empty;
            if (prefix != null) chat = prefix;
            for (var i = 0; i < commandsToSend.Count; i++)
            {
                chat += commandsToSend[i];
                if (i < commandsToSend.Count-1) chat += $", ";
            }

            chatUI.SendChatMessage(chat);
        }
        public List<string> getAutoFill(int paramIndex)
        {
            return ChatCommandFactory.getAllCommands();
        }
        
        public int getPages() {
            List<string> commands = ChatCommandFactory.getAllCommands();
            return Mathf.CeilToInt(commands.Count/(float) COMMANDS_PER_PAGE);
        }

        public override string getDescription()
        {
            List<string> commands = ChatCommandFactory.getAllCommands();
            int pages = Mathf.CeilToInt(commands.Count/COMMANDS_PER_PAGE);
            string output = $"Displays a list of all available commands. Optional parameter: specify a page number between 1 and {pages+1} to view commands in segments. Enter a command to receive a detailed description.";
            return output;
        }
    }
}

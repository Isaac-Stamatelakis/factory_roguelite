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
            List<string> stringCommands = ChatCommandFactory.getAllCommands();
            stringCommands.Sort();
            if (parameters.Length == 0) {
                string output = "";
                foreach (string stringCommand in stringCommands) {
                    output += stringCommand + ", ";
                }
                TextChatUI.Instance.sendMessage(output);
            } else {
                try {
                    int page = Convert.ToInt32(parameters[0]);
                    int helpPages = getPages();
                    page --; // Page 1 -> 0, 2 -> 1, ...
                    if (page < 0 || page * helpPages >= stringCommands.Count) {
                        chatUI.sendMessage("Page is not between 1 and " + helpPages);
                        return;
                    }
                    string output = "";
                    for (int i = 0; i < helpPages; i++) {
                        int index = page * helpPages + i;
                        if (index >= stringCommands.Count) {
                            break;
                        }
                        output += stringCommands[index].ToString() +  ", ";
                    }
                    chatUI.sendMessage(output);
                    
                } catch (FormatException) {
                    /*
                    ChatCommand? paramCommand = ChatCommandFactory.getCommand(parameters[0]);
                    if (paramCommand == null) {
                        chatUI.sendMessage("Could not find command '" + parameters[0] + "'");
                        return;
                    }
                    chatUI.sendMessage(((ChatCommand)paramCommand).getDescription());
                    */
                } catch (OverflowException) {
                    chatUI.sendMessage("Page is too large");
                }
            }
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using PlayerModule;
using UI.QuestBook;
using WorldModule;

namespace UI.Chat {
    public class QuestCheatCommand : ChatCommand
    {
        public QuestCheatCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            if (parameters.Length > 0)
            {
                bool state = ChatCommandParameterParser.parseBool(parameters,0,"state");
                QuestBookUtils.EditMode = state;
                return;
            }
            QuestBookUtils.EditMode = !QuestBookUtils.EditMode;
            
        }

        public override string getDescription()
        {
            return "/quest_cheat (optional 'boolean') \nToggles cheat mode";
        }
    }
    
    public class QuestResetCommand : ChatCommand, IAutoFillChatCommand
    {
        public QuestResetCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            string stage = parameters[0];
            if (parameters.Length < 2)
            {
                throw new ChatParseException("--force is required for command 'quest_set'");
            }

            if (parameters[1] != "--force")
            {
                throw new ChatParseException($"'{parameters[1]}' is not --force");
            }
            
            
            chatUI.SendChatMessage($"<color=green>Set quest book to stage '{stage}'</color>");
        }
        

        public override string getDescription()
        {
            return "/quest_set 'stage' --force\n";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            switch (paramIndex)
            {
                case 0:
                    return new List<string>() { "0" };
                default:
                    return new List<string>();
            }
        }
    }
}


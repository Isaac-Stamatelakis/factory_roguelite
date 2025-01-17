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
                QuestBookHelper.EditMode = state;
                return;
            }
            QuestBookHelper.EditMode = !QuestBookHelper.EditMode;
            
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
            Debug.Log(parameters.Length);
            string stage = parameters[0];
            if (parameters.Length < 2)
            {
                throw new ChatParseException("--force is required for command 'quest_set'");
            }

            if (parameters[1] != "--force")
            {
                throw new ChatParseException($"'{parameters[1]}' is not --force");
            }
            
            WorldManager.getInstance().SetQuestBookFromJson(GetJson(stage));
            chatUI.sendMessage($"<color=green>Set quest book to stage '{stage}'</color>");
        }

        private string GetJson(string stage)
        {
            return stage switch
            {
                "0" => File.ReadAllText(QuestBookHelper.DEFAULT_QUEST_BOOK_PATH),
                _ => throw new ChatParseException($"unknown stage 'stage'")
            };
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


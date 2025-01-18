using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;
using WorldModule;

namespace UI.Chat {
    public class GameStageCommand : ChatCommand, IAutoFillChatCommand
    {
        public GameStageCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            string mode = parameters[0].ToLower();
            
            switch (mode)
            {
                case "add":
                case "remove":
                    string stage = parameters[1];
                    if (mode == "add")
                    {
                        WorldManager.getInstance().UnlockGameStage(stage);
                    }
                    else
                    {
                        WorldManager.getInstance().RemoveGameStage(stage);
                    }
                    break;
                case "enable":
                    DevMode.Instance.EnableGameStages = true;
                    break;
                case "disable":
                    DevMode.Instance.EnableGameStages = false;
                    break;
                default:
                    TextChatUI.Instance.sendMessage("Invalid mode. Use 'add' or 'remove'.");
                    break;
            }
        }

        public override string getDescription()
        {
            return "/gamestage ('add' or 'remove') 'STAGENAME'\nModifies gamestage meta data of current world";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            return paramIndex switch
            {
                0 => new List<string>() { "add", "remove" },
                1 => new List<string>() { "'STAGENAME' (case sensitive)" },
                _ => new List<string>() { }
            };
        }
    }
}


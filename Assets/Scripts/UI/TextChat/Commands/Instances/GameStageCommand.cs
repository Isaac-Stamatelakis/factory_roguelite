using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Player;
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
            PlayerScript playerScript = PlayerManager.Instance.GetPlayer();
            switch (mode)
            {
                case "add":
                case "remove":
                    string stage = parameters[1];
                    if (mode == "add")
                    {
                        playerScript.GameStageCollection.UnlockedStages.Add(stage);
                    }
                    else
                    {
                        playerScript.GameStageCollection.UnlockedStages.Remove(stage);
                    }
                    break;
                case "print":
                    string message = "Unlocked Stages: ";
                    foreach (string unlockedStage in playerScript.GameStageCollection.UnlockedStages)
                    {
                        message += $"'{unlockedStage}' ";
                    }
                    chatUI.SendChatMessage(message);
                    break;
                case "enable":
                    DevMode.Instance.EnableGameStages = true;
                    break;
                case "disable":
                    DevMode.Instance.EnableGameStages = false;
                    break;
                default:
                    TextChatUI.Instance.SendChatMessage("Invalid mode. Use 'add' or 'remove'.");
                    break;
            }
        }

        public override string getDescription()
        {
            List<string> firstOptions = getAutoFill(0);
            
            return $"/gamestage {ChatCommandParameterParser.FormatParameters(firstOptions)} (stage)\nModifies gamestage meta data of current world";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            return paramIndex switch
            {
                0 => new List<string>() { "add", "remove", "print", "enable" , "disable" },
                1 => new List<string>() { "'STAGENAME' (case sensitive)" },
                _ => new List<string>() { }
            };
        }
    }
}


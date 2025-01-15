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
            try {
                string mode = parameters[0].ToLower();
                string stage = parameters[1];
                switch (mode)
                {
                    case "add":
                        WorldManager.getInstance().UnlockGameStage(stage);
                        break;
                    case "remove":
                        WorldManager.getInstance().RemoveGameStage(stage);
                        break;
                    default:
                        TextChatUI.Instance.sendMessage("Invalid mode. Use 'add' or 'remove'.");
                        break;
                }
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
            catch (FormatException e) {
                TextChatUI.Instance.sendMessage(e.ToString());
            } catch (OverflowException e) {
                TextChatUI.Instance.sendMessage(e.ToString());
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;
using WorldModule;

namespace UI.Chat {
    public class HealCommand : ChatCommand
    {
        public HealCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            float amount = ChatCommandParameterParser.ParseFloat(parameters, 0, "amount");
            if (amount < 0) throw new ChatParseException("Healing amount must be a non-negative number.");
            PlayerManager.Instance.GetPlayer().PlayerRobot.Heal(amount);
            
        }

        public override string getDescription()
        {
            return "/heal (amount) \nHeals player robot by amount";
        }

        
    }
}


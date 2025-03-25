using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;
using WorldModule;

namespace UI.Chat {
    public class DamageCommand : ChatCommand
    {
        public DamageCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            float amount = ChatCommandParameterParser.ParseFloat(parameters, 0, "amount");
            if (amount < 0) throw new ChatParseException("Damage amount must be positive number.");
            PlayerManager.Instance.GetPlayer().PlayerRobot.Damage(amount);
        }

        public override string getDescription()
        {
            return "/damage (amount) \nDamages player robot by amount";
        }

        
    }
}


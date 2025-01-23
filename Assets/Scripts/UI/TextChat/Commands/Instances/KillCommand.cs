using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;

namespace UI.Chat {
    public class KillCommand : ChatCommand
    {
        public KillCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            PlayerManager.Instance.GetPlayer().PlayerRobot.Die();
        }

        public override string getDescription()
        {
            return "/kill \nKills the player";
        }
    }
}


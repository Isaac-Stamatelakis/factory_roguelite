using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
using Player;
using UnityEngine.Rendering.Universal;

namespace UI.Chat {
    public class RecallCommand : ChatCommand
    {
        public RecallCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            PlayerScript player = PlayerManager.Instance.GetPlayer();
            DimensionManager.Instance.SetPlayerSystem(player,0,new Vector2(0,0));
        }

        public override string getDescription()
        {
            return "/recall \nTeleports player to hub at 0 0";
        }
    }
}

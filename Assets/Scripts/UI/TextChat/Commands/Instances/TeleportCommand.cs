using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayerModule;

namespace UI.Chat {
    public class TeleportCommand : ChatCommand
    {
        public TeleportCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            Vector3 playerPosition = playerTransform.position;
            playerPosition.x = ChatCommandParameterParser.ParseFloat(parameters,0,"x",playerPosition.x);
            playerPosition.y = ChatCommandParameterParser.ParseFloat(parameters,1,"y",playerPosition.y);
            
            playerTransform.position = playerPosition;
        }

        public override string getDescription()
        {
            return "/teleport (x|~offset) (y|~offset)\nTeleports the player to the specified coordinates (x, y) within the current dimension.";
        }
    }
}


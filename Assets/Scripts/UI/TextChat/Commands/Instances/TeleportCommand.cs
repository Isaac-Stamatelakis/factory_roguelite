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
            int x = Convert.ToInt32(parameters[0]);
            int y = Convert.ToInt32(parameters[1]);
            Transform playerTransform = PlayerManager.Instance.GetPlayer().transform;
            Vector3 playerPosition = playerTransform.position;
            playerPosition.x = x;
            playerPosition.y = y;
            playerTransform.position = playerPosition;
        }

        public override string getDescription()
        {
            return "/teleport x y\nTeleports the player to the specified coordinates (x, y) within the current dimension.";
        }
    }
}


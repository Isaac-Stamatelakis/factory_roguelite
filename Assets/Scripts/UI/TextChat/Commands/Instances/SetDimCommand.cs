using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Items;
using UnityEngine.Rendering.Universal;

namespace UI.Chat {
    public class SetDimCommand : ChatCommand
    {
        public SetDimCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            try {
                int dim = ChatCommandParameterParser.parseInt(parameters,0,"dim");
                Transform player = PlayerContainer.getInstance().getTransform();
                Vector2 position = player.transform.position;
                if (parameters.Length > 1) {
                    position.x = ChatCommandParameterParser.parseInt(parameters,1,"x");
                    position.y = ChatCommandParameterParser.parseInt(parameters,2,"y");
                }
                DimensionManager.Instance.setPlayerSystem(player,(int)dim,new Vector2Int((int)position.x,(int)position.y));
                
                
            } catch (ChatParseException e) {
                chatUI.sendMessage(e.Message);
            }
        }

        public override string getDescription()
        {
            return "/setdim dim position\nTeleports player to dimension at position";
        }
    }
}

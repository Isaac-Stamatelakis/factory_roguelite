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
    public class SetDimCommand : ChatCommand
    {
        public SetDimCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            Dimension dim = (Dimension) ChatCommandParameterParser.ParseInt(parameters,0,"dim",null);
            PlayerScript player = PlayerManager.Instance.GetPlayer();
            Vector2 position = player.transform.position;
            if (parameters.Length > 1) {
                position.x = ChatCommandParameterParser.ParseFloat(parameters,1,"x",position.x);
                position.y = ChatCommandParameterParser.ParseFloat(parameters,2,"y",position.y);
            }
            DimensionManager.Instance.SetPlayerSystem(player,dim,new Vector2Int((int)position.x,(int)position.y));
        }

        public override string getDescription()
        {
            return "/setdim (-1|0|1) (position)?\nTeleports player to dimension at position";
        }
    }
}

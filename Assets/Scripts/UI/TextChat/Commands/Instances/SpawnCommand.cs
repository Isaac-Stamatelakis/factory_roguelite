using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using PlayerModule;
using Entities;
using Entities.Mobs;

namespace UI.Chat {
    public class SpawnCommand : ChatCommand, IAutoFillChatCommand
    {
        public SpawnCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            try {
                Vector3 playerPosition = PlayerContainer.getInstance().getTransform().position;
                string id = parameters[0];
                Transform player = PlayerContainer.getInstance().getTransform();
                int dim = DimensionManager.Instance.getPlayerDimension(player);
                DimController dimController = DimensionManager.Instance.getDimController(dim);
                EntityRegistry.getInstance().spawnEntity(id,playerPosition,null,dimController.EntityContainer);
                
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            }
        }

        public List<string> getAutoFill(int paramIndex)
        {
            return EntityUtils.getAllIds();
        }

        public override string getDescription()
        {
            return "/spawn id\nSpawns entity with id at player location";
        }
    }
}

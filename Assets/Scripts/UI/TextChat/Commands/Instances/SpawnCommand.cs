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
            Vector3 playerPosition = PlayerManager.Instance.GetPlayer().transform.position;
            string id = parameters[0];
            int amount = 1;
            if (parameters.Length > 1)
            {
                amount = ChatCommandParameterParser.ParseInt(parameters, 1, "amount",1);
            }
         
            int dim = DimensionManager.Instance.GetPlayerDimension();
            DimController dimController = DimensionManager.Instance.GetDimController((Dimension)dim);
            SerializedMobEntityData spawnEntityData = new SerializedMobEntityData
            {
                Id = id,
                Health = float.MinValue
            };
            while (amount > 0)
            {
                EntityRegistry.Instance.StartEntitySpawnCoroutine(spawnEntityData, playerPosition,dimController.GetActiveSystem()?.EntityContainer);
                amount--;
            }
            
            
        }

        public List<string> getAutoFill(int paramIndex)
        {
            if (paramIndex > 0) return new List<string>();
            return EntityRegistry.Instance.GetAllMobIds();
        }

        public override string getDescription()
        {
            return "/spawn (id) (amount)?\nSpawns entity with id at player location";
        }
    }
}

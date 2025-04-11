using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Dimensions;
using Entities;
using Entities.Mobs;
using PlayerModule;

namespace UI.Chat {
    public class KillCommand : ChatCommand, IAutoFillChatCommand
    {
        private readonly Dictionary<string, Action<string[],TextChatUI>> funcOptionMap = new Dictionary<string, Action<string[],TextChatUI>>
        {
            { "self", KillSelf },
            { "items", KillItems}
        };
        public KillCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }

        public override void execute()
        {
            string type = parameters[0];
            if (!funcOptionMap.TryGetValue(type, out Action<string[], TextChatUI> func))
            {
                throw new ChatParseException("Unknown subcommand '" + type + "'");
            }
            func.Invoke(parameters,chatUI);
        }

        private static void KillSelf(string[] parameters,TextChatUI textChatUI)
        {
            PlayerManager.Instance.GetPlayer().PlayerRobot.Die();
        }

        private static void KillItems(string[] parameters, TextChatUI textChatUI)
        {
            Transform player = PlayerManager.Instance.GetPlayer().transform;
            int dim = DimensionManager.Instance.GetPlayerDimension();
            DimController dimController = DimensionManager.Instance.GetDimController((Dimension)dim);
            Transform entityContainer = dimController.GetActiveSystem()?.EntityContainer;
            if (!entityContainer) return;
            for (int i = 0; i < entityContainer.childCount; i++)
            {
                Transform child = entityContainer.GetChild(i);
                if (child.tag == "ItemEntity") GameObject.Destroy(child.gameObject);
            }
        }
        public override string getDescription()
        {
            return "/kill ('self' or 'items')\nKills objects";
        }

        public List<string> getAutoFill(int paramIndex)
        {
            return funcOptionMap.Keys.ToList();
        }
    }
}


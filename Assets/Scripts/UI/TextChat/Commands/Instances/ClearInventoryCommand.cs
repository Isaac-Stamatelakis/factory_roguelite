using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using Item.Slot;
using PlayerModule;
using Items;
using Recipe.Data;
using TileEntity.Instances.Machine.UI;

namespace UI.Chat {
    public class ClearInventoryCommand : ChatCommand
    {
        public ClearInventoryCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            for (int i = 0; i < playerInventory.Inventory.Count; i++)
            {
                playerInventory.Inventory[i] = null;
            }
            playerInventory.Refresh();
        }
        

        public override string getDescription()
        {
            return "/clear\nClears the players inventory";
        }
    }
}

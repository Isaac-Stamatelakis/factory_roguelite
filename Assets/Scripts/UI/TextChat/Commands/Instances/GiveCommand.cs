using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
using Item.Slot;
using PlayerModule;
using Items;
using Recipe.Data;

namespace UI.Chat {
    public class GiveCommand : ChatCommand, IAutoFillChatCommand
    {
        public GiveCommand(string[] parameters, TextChatUI textChatUI) : base(parameters, textChatUI)
        {
        }
        public override void execute()
        {
            string id = parameters[0];
            uint amount = Convert.ToUInt32(parameters[1]);
            if (amount == 0) return;
            
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
            if (ReferenceEquals(itemObject,null)) throw new ChatParseException($"Could not find item with id: '{id}'");
            while (amount > 0)
            {
                uint amountInSlot = amount > Global.MaxSize ? Global.MaxSize : amount;
                amount -= amountInSlot;
                ItemSlot toGive = new ItemSlot(itemObject, amountInSlot, null);
                playerInventory.Give(toGive);
            }
        }

        public List<string> getAutoFill(int paramIndex)
        {
            if (paramIndex != 0) {
                return new List<string>();
            }
            List<ItemObject> items = ItemRegistry.GetInstance().GetAllItems();
            List<string> ids = new List<string>();
            foreach (ItemObject item in items) {
                ids.Add(item.id);
            }
            return ids;
        }

        public override string getDescription()
        {
            return "/give id amount\nGives player amount of item with id";
        }
    }
}

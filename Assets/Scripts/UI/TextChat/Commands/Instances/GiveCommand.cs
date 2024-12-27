using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Dimensions;
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
            try {
                string id = parameters[0];
                uint amount = Convert.ToUInt32(parameters[1]);
                amount = GlobalHelper.Clamp(amount, 1, Global.MaxSize);
                PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
                ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(id);
                ItemSlot toGive = ItemSlotFactory.CreateNewItemSlot(itemObject,(uint)amount);
                if (toGive == null) {
                    chatUI.sendMessage("Invalid id");
                    return;
                }
                playerInventory.give(toGive);
            } catch (IndexOutOfRangeException) {
                chatUI.sendMessage("Invalid parameter format");
            } catch (FormatException) {
                chatUI.sendMessage("Amount is not a number");
            } catch (OverflowException) {
                chatUI.sendMessage("Amount is too large");
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

using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using PlayerModule;
using UnityEngine;

namespace UI.QuestBook {
    public class ItemQuestTask : QuestBookTask
    {
        public List<SerializedItemSlot> Items;
        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Item;
        }
    }

    public class ItemQuestTaskData : QuestBookTaskData
    {
        public List<SerializedItemSlot> Items;
       /*
        
        public bool CheckCompletion()
        {
            bool complete = IsComplete();
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            
            for (var i = 0; i < items.Count; i++)
            {
                var serializedItemSlot = items[i];
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
                uint amount = ItemSlotUtils.AmountOf(itemSlot, playerInventory.Inventory);
                if (amount > gottenAmounts[i]) gottenAmounts[i] = amount;
            }
            return complete != IsComplete(); // Return true if value is changed
        }
        */
        
    }
}


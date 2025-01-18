using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using PlayerModule;
using UnityEngine;

namespace UI.QuestBook {
    public class ItemQuestTask : QuestBookTask, ICompletionCheckQuest
    {
        private List<SerializedItemSlot> items;
        private List<uint> gottenAmounts;

        public List<SerializedItemSlot> Items { get => items; set => items = value; }
        public List<uint> GottenAmounts { get => gottenAmounts; set => gottenAmounts = value; }

        public override bool IsComplete()
        {
            for (int i = 0; i < gottenAmounts.Count; i++) {
                if (gottenAmounts[i] < items[i].amount) {
                    return false;
                }
            }
            return true;
        }
        public ItemQuestTask() {
            this.items = new List<SerializedItemSlot>();
            this.gottenAmounts = new List<uint>();
        }

        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Item;
        }

        public override void SetCompletion(bool state)
        {
            while (gottenAmounts.Count > items.Count) {
                gottenAmounts.RemoveAt(gottenAmounts.Count-1);
            }
            while (gottenAmounts.Count < items.Count) {
                gottenAmounts.Add(0);
            }
            
            for (int i = 0; i < items.Count; i++) {
                if (state)
                {
                    gottenAmounts[i] = items[i].amount;
                }
                else
                {
                    gottenAmounts[i] = 0;
                }
            }
        }
        
        public void CheckCompletion()
        {
            PlayerInventory playerInventory = PlayerManager.Instance.GetPlayer().PlayerInventory;
            
            for (var i = 0; i < items.Count; i++)
            {
                var serializedItemSlot = items[i];
                ItemSlot itemSlot = ItemSlotFactory.deseralizeItemSlot(serializedItemSlot);
                uint amount = ItemSlotUtils.AmountOf(itemSlot, playerInventory.Inventory);
                if (amount > gottenAmounts[i]) gottenAmounts[i] = amount;
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class ItemQuestTask : QuestBookTask
    {
        private List<SerializedItemSlot> items;
        private List<int> gottenAmounts;

        public List<SerializedItemSlot> Items { get => items; set => items = value; }
        public List<int> GottenAmounts { get => gottenAmounts; set => gottenAmounts = value; }

        public override bool getComplete()
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
            this.gottenAmounts = new List<int>();
        }

        public override QuestTaskType getTaskType()
        {
            return QuestTaskType.Item;
        }
    }
}


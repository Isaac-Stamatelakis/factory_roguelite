using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class ItemQuestTask : QuestBookTask
    {
        private List<SerializedItemSlot> items;
        private List<uint> gottenAmounts;

        public List<SerializedItemSlot> Items { get => items; set => items = value; }
        public List<uint> GottenAmounts { get => gottenAmounts; set => gottenAmounts = value; }

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
            this.gottenAmounts = new List<uint>();
        }

        public override QuestTaskType getTaskType()
        {
            return QuestTaskType.Item;
        }

        public override void setComplete()
        {
            balanceLists();
            for (int i = 0; i < items.Count; i++) {
                gottenAmounts[i] = items[i].amount;
            }
        }

        private void balanceLists() {
            while (gottenAmounts.Count > items.Count) {
                gottenAmounts.RemoveAt(gottenAmounts.Count-1);
            }
            while (gottenAmounts.Count < items.Count) {
                gottenAmounts.Add(0);
            }
        }

        public override void setUnComplete()
        {
            balanceLists();
            for (int i = 0; i < gottenAmounts.Count; i++) {
                gottenAmounts[i] = 0;
            }
        }
    }
}


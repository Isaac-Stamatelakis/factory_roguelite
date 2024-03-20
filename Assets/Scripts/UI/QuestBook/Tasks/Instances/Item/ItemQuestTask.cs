using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class ItemQuestTask : QuestBookTask
    {
        private List<SerializedItemSlot> items;
        private List<int> gottenAmounts;

        public override bool getComplete()
        {
            for (int i = 0; i < gottenAmounts.Count; i++) {
                if (gottenAmounts[i] < items[i].amount) {
                    return false;
                }
            }
            return true;
        }
    }
}


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
        public ItemQuestTaskData(bool complete, QuestBookRewardClaimStatus rewardStatus, List<SerializedItemSlot> items) : base(complete, rewardStatus)
        {
           Items = items;
        }
    }
}


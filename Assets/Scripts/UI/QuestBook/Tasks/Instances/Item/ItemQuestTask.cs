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

        public ItemQuestTask(List<SerializedItemSlot> items)
        {
            Items = items;
        }
    }

    public class ItemQuestTaskData : ExtraTaskData
    { 
        public List<SerializedItemSlot> Items;
        public ItemQuestTaskData(List<SerializedItemSlot> items)
        {
           Items = items;
        }
    }
}


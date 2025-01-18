using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTask
    {
        protected List<SerializedItemSlot> rewards;
        public List<SerializedItemSlot> Rewards { get => rewards; set => rewards = value; }
        public abstract QuestTaskType GetTaskType();
        public abstract bool IsComplete();
        public abstract void SetCompletion(bool state);
    }

    public interface ICompletionCheckQuest
    {
        public void CheckCompletion();
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTask
    {
        protected List<SerializedItemSlot> rewards;
        public List<SerializedItemSlot> Rewards { get => rewards; set => rewards = value; }
        public bool RewardsClaimed { get => rewardsClaimed; set => rewardsClaimed = value; }

        protected bool rewardsClaimed = false;

        public abstract QuestTaskType getTaskType();
        public abstract bool getComplete();
        public abstract void setComplete();
        public abstract void setUnComplete();
    }
}


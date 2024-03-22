using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookNodeContent
    {
        private QuestBookTask task;
        private string description;
        private string title;
        private int numberOfRewards;
        private List<SerializedItemSlot> rewards;

        public QuestBookTask Task { get => task; set => task = value; }
        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public int NumberOfRewards { get => numberOfRewards; set => numberOfRewards = value; }
        public List<SerializedItemSlot> Rewards { get => rewards; set => rewards = value; }

        public QuestBookNodeContent(QuestBookTask task, string description, string title, List<SerializedItemSlot> rewards, int numberOfRewards) {
            this.task = task;
            this.description = description;
            this.title = title;
            this.rewards = rewards;
            this.numberOfRewards = numberOfRewards;
        }
    }

}

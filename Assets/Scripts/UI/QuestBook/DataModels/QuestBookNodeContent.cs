using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookNodeContent
    {
        private QuestBookTask task;
        private string description;
        private string title;
        private List<SerializedItemSlot> rewards;
        private List<QuestBookCommandReward> commandRewards;
        private int numberOfRewards;

        public QuestBookTask Task { get => task; set => task = value; }
        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public List<SerializedItemSlot> Rewards { get => rewards; set => rewards = value; }
        public List<QuestBookCommandReward> CommandRewards { get => commandRewards; set => commandRewards = value; }
        public int NumberOfRewards { get => numberOfRewards; set => numberOfRewards = value; }

        public QuestBookNodeContent(QuestBookTask task, string description, string title, List<SerializedItemSlot> rewards, List<QuestBookCommandReward> commandRewards, int numberOfRewards) {
            this.task = task;
            this.description = description;
            this.title = title;
            this.rewards = rewards;
            this.commandRewards = commandRewards;
            this.numberOfRewards = numberOfRewards;
        }
    }

    public class QuestBookCommandReward
    {
        public string Description;
        public string Command;

        public QuestBookCommandReward(string description, string command)
        {
            Description = description;
            Command = command;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookNodeContent
    {
        public QuestBookTask Task;
        public string Description;
        public string Title;
        private List<SerializedItemSlot> rewards;
        public QuestBookItemRewards ItemRewards;
        public QuestBookCommandRewards CommandRewards;
        public QuestBookNodeContent(QuestBookTask task, string description, string title, QuestBookItemRewards itemRewards, QuestBookCommandRewards commandRewards) {
            this.Task = task;
            this.Description = description;
            this.Title = title;
            this.ItemRewards = itemRewards;
            this.CommandRewards = commandRewards;
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

    public class QuestBookReward
    {
        public bool Claimed;
    }

    public class QuestBookItemRewards : QuestBookReward
    {
        public int Selectable;
        public List<SerializedItemSlot> Rewards;

        public QuestBookItemRewards(List<SerializedItemSlot> rewards, int selectable)
        {
            Rewards = rewards;
            Selectable = selectable;
        }
    }

    public class QuestBookCommandRewards : QuestBookReward
    {
        public List<QuestBookCommandReward> CommandRewards;

        public QuestBookCommandRewards(List<QuestBookCommandReward> commandRewards)
        {
            CommandRewards = commandRewards;
        }
    }

}

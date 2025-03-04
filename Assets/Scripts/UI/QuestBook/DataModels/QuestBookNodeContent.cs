using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UI.QuestBook.Tasks;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookNodeContent
    {
        public QuestBookNodeSize Size;
        [JsonIgnore] public QuestBookTask Task;
        public string Description;
        public string Title;
        private List<SerializedItemSlot> rewards;
        public QuestBookItemRewards ItemRewards;
        public QuestBookCommandRewards CommandRewards;
        public QuestBookNodeContent(QuestBookTask task, string description, string title, QuestBookItemRewards itemRewards, QuestBookCommandRewards commandRewards, QuestBookNodeSize size) {
            this.Task = task;
            this.Description = description;
            this.Title = title;
            this.ItemRewards = itemRewards;
            this.CommandRewards = commandRewards;
            this.Size = size;
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

        public bool TryClaim()
        {
            if (!Claimed) return Claimed;
            if (QuestBookUtils.EditMode)
            {
                Claimed = false;
            }

            return Claimed;
        }
    }

    
    public class QuestBookItemRewards : QuestBookReward
    {
        public bool LimitOne;
        public List<SerializedItemSlot> Rewards;

        public QuestBookItemRewards(List<SerializedItemSlot> rewards, bool limitOne)
        {
            Rewards = rewards;
            LimitOne = limitOne;
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

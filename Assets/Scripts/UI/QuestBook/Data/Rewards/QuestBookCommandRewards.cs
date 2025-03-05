using System.Collections.Generic;

namespace UI.QuestBook.Data.Rewards
{
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

    public class QuestBookCommandRewards : QuestBookReward
    {
        public List<QuestBookCommandReward> CommandRewards;

        public QuestBookCommandRewards(List<QuestBookCommandReward> commandRewards)
        {
            CommandRewards = commandRewards;
        }
    }
}
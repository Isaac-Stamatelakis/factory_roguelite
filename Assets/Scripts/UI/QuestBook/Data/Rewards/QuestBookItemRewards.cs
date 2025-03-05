using System.Collections.Generic;

namespace UI.QuestBook.Data.Rewards
{
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
}
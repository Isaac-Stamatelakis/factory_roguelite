namespace UI.QuestBook.Data.Node
{
    public class QuestBookTaskData
    {
        public bool Complete;
        public int Id;
        public QuestBookRewardClaimStatus RewardStatus;
        public QuestBookTaskData(bool complete, QuestBookRewardClaimStatus rewardStatus, int id)
        {
            Complete = complete;
            RewardStatus = rewardStatus;
            Id = id;
        }
    }
}
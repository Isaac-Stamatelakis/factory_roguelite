using System.Collections.Generic;
using Newtonsoft.Json;
using UI.QuestBook.Data.Rewards;

namespace UI.QuestBook.Data.Node
{
    public class QuestBookNodeContent
    {
        [JsonIgnore] public QuestBookTask Task;
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
}
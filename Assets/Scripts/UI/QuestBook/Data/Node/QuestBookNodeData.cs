using System.Collections.Generic;
using UI.QuestBook.Tasks;

namespace UI.QuestBook.Data.Node
{
    public class QuestBookNodeData
    {
        public List<int> Prerequisites;
        public SerializedItemSlot ImageSeralizedItemSlot;
        public float X;
        public float Y;
        public bool RequireAllPrerequisites;
        public int Id;
        public QuestBookNodeContent Content;
        public QuestBookNodeSize Size;

        public QuestBookNodeData(List<int> prerequisites, SerializedItemSlot imageSeralizedItemSlot, float x, float y, bool requireAllPrerequisites, int id, QuestBookNodeSize size, QuestBookNodeContent content)
        {
            Prerequisites = prerequisites;
            ImageSeralizedItemSlot = imageSeralizedItemSlot;
            X = x;
            Y = y;
            RequireAllPrerequisites = requireAllPrerequisites;
            Id = id;
            Content = content;
        }
    }
}
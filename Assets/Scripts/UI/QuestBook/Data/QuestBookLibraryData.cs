using System.Collections.Generic;

namespace UI.QuestBook.Data
{
    public class QuestBookLibraryData
    {
        public List<QuestBookSelectorData> QuestBookDataList;

        public QuestBookLibraryData(List<QuestBookSelectorData> questBookDataList)
        {
            QuestBookDataList = questBookDataList;
        }

        public object Path { get; set; }
    }
}
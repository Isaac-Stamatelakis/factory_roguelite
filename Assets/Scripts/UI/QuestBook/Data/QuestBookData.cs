using System.Collections.Generic;

namespace UI.QuestBook.Data
{
    public class QuestBookData {
        public int IDCounter;
        public List<QuestBookPageData> PageDataList;
        public QuestBookData(int idCounter, List<QuestBookPageData> pageDataList) {
            this.IDCounter = idCounter;
            this.PageDataList = pageDataList;
        }
    }
}
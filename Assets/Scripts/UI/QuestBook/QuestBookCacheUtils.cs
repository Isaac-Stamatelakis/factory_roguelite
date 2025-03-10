using System;
using System.Collections.Generic;

namespace UI.QuestBook
{
    public class QuestBookCache
    {
        private struct QuestCacheData
        {
            public QuestTaskType QuestTaskType;
            public string CacheData;

            public QuestCacheData(QuestTaskType questTaskType, string cacheData)
            {
                QuestTaskType = questTaskType;
                CacheData = cacheData;
            }
        }
        private List<QuestCacheData> cacheList = new List<QuestCacheData>();
        private const int MAX_CACHE_SIZE = 3;
        
        public void CacheQuestData(string data, QuestTaskType questTaskType)
        {
            cacheList.Insert(0,new QuestCacheData(questTaskType, data));
            if (cacheList.Count >= MAX_CACHE_SIZE) cacheList.RemoveAt(cacheList.Count - 1);
        }

        public bool QueueSatisfiedCache(QuestTaskType questTaskType, Func<string, bool>  success)
        {
            foreach (QuestCacheData cacheData in cacheList)
            {
                if (cacheData.QuestTaskType != questTaskType) continue;
                if (success.Invoke(cacheData.CacheData)) return true;
            }
            return false;
        }
       
    }
}

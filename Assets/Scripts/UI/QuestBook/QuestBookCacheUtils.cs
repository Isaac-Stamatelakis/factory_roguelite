using System.Collections.Generic;

namespace UI.QuestBook
{
    public class QuestBookCache
    {
        private Queue<string> cacheQueue = new Queue<string>();
        private const int MAX_CACHE_SIZE = 3;
        public static bool IsTask(string cacheData, QuestTaskType type)
        {
            return cacheData.StartsWith(type.ToString().ToUpper());
        }

        public static string CacheTask(string data, QuestTaskType type)
        {
            return type.ToString().ToUpper() + data;
        }
        
        public void CacheQuestData(string data, QuestTaskType questTaskType)
        {
            cacheQueue.Enqueue(CacheTask(data, questTaskType));
            if (cacheQueue.Count >= MAX_CACHE_SIZE) cacheQueue.Dequeue();
            
        }

        public bool MatchString(QuestTaskType questTaskType, string data)
        {
            foreach (string cacheKey in cacheQueue)
            {
                if (!IsTask(cacheKey, questTaskType)) continue;
                if (cacheKey.Contains(data)) return true;
            }

            return false;
        }
    }
}

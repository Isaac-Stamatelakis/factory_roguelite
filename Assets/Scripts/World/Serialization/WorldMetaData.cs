using System;
using System.Collections.Generic;

namespace World.Serialization
{
    public class WorldMetaData
    {
        public DateTime CreationDate;
        public DateTime LastAccessDate;
        public bool CheatsEnabled;
        public string QuestBook;
        public WorldMetaData(DateTime creationDate, DateTime lastAccessDate, bool cheatsEnabled, string questBook)
        {
            CreationDate = creationDate;
            LastAccessDate = lastAccessDate;
            CheatsEnabled = cheatsEnabled;
            QuestBook = questBook;
        }
    }
}

using System;
using System.Collections.Generic;

namespace World.Serialization
{
    public class WorldMetaData
    {
        public DateTime CreationDate;
        public DateTime LastAccessDate;
        public List<string> UnlockedGameStages;

        public WorldMetaData(DateTime creationDate, DateTime lastAccessDate, List<string> unlockedGameStages)
        {
            CreationDate = creationDate;
            LastAccessDate = lastAccessDate;
            UnlockedGameStages = unlockedGameStages;
        }
    }
}

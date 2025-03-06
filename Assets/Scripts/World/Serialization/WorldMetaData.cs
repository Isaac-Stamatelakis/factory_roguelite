using System;
using System.Collections.Generic;

namespace World.Serialization
{
    public class WorldMetaData
    {
        public DateTime CreationDate;
        public DateTime LastAccessDate;
        public bool CheatsEnabled;
        public WorldMetaData(DateTime creationDate, DateTime lastAccessDate, bool cheatsEnabled)
        {
            CreationDate = creationDate;
            LastAccessDate = lastAccessDate;
            CheatsEnabled = cheatsEnabled;
        }
    }
}

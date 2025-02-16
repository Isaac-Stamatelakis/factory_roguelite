using System;

namespace UI.TitleScreen.Select
{
    public struct WorldDisplayData
    {
        public bool Corrupted;
        public string Name;
        public DateTime? CreateTime;
        public DateTime? LastAccessTime;

        public WorldDisplayData(string name, DateTime? createTime, DateTime? lastAccessTime, bool corrupted)
        {
            Name = name;
            CreateTime = createTime;
            LastAccessTime = lastAccessTime;
            Corrupted = corrupted;
        }
    }
}

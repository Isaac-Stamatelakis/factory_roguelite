using System;

namespace UI.TitleScreen.Select
{
    public struct WorldDisplayData
    {
        public string Name;
        public DateTime CreateTime;
        public DateTime LastAccessTime;

        public WorldDisplayData(string name, DateTime createTime, DateTime lastAccessTime)
        {
            Name = name;
            CreateTime = createTime;
            LastAccessTime = lastAccessTime;
        }
    }
}

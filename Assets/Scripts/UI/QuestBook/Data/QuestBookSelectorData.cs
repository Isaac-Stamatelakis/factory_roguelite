namespace UI.QuestBook.Data
{
    public enum QuestBookTitleSpritePath
    {
        Stars = 0,
        BluePlanet = 1,
        PurplePlanet = 2,
        Teleporter = 3
    }
    public class QuestBookSelectorData
    {
        public string Title;
        public QuestBookTitleSpritePath SpritePath;
        public string Id;

        public QuestBookSelectorData(string title, QuestBookTitleSpritePath spritePath, string id)
        {
            Title = title;
            SpritePath = spritePath;
            Id = id;
        }
    }
}
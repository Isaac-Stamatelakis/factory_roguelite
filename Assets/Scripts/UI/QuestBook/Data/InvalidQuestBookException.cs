using System;

namespace UI.QuestBook.Data
{
    public class InvalidQuestBookException : Exception
    {
        public InvalidQuestBookException(string message) : base(message)
        {
        }
    }
}
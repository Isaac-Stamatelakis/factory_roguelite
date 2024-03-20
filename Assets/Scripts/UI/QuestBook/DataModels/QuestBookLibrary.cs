using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UI.QuestBook {
    /// <summary>
    /// A collection of quest books
    /// </summary>
    public class QuestBookLibrary
    {
        private List<QuestBook> questBooks;

        public List<QuestBook> QuestBooks { get => questBooks; set => questBooks = value; }
        public QuestBookLibrary(List<QuestBook> books) {
            this.questBooks = books;
        }
    }
}


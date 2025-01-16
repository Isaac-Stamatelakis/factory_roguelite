using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBook
    {
        public List<QuestBookPage> Pages;
        public string Title;
        public string SpritePath;
        public QuestBook(List<QuestBookPage> pages, string title, string spritePath) {
            this.Title = title;
            this.Pages = pages;
            this.SpritePath = spritePath;
        }
    }
}


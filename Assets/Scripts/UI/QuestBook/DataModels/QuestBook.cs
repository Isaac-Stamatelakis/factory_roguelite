using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBook
    {
        private List<QuestBookPage> pages;
        private string title;
        private string spritePath;
        public QuestBook(List<QuestBookPage> pages, string title, string spritePath) {
            this.title = title;
            this.pages = pages;
            this.spritePath = spritePath;
        }

        public List<QuestBookPage> Pages { get => pages; set => pages = value; }
        public string Title { get => title; set => title = value; }
        public string SpriteKey { get => spritePath; set => spritePath = value; }
    }
}


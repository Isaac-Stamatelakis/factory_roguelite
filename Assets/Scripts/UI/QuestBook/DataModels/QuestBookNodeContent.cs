using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookNodeContent
    {
        private QuestBookTask task;
        private string description;
        private string title;

        public QuestBookTask Task { get => task; set => task = value; }
        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public QuestBookNodeContent(QuestBookTask task, string description, string title) {
            this.task = task;
            this.description = description;
            this.title = title;
        }
    }

}

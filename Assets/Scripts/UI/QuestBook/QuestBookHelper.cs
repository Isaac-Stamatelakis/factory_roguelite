using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookHelper 
    {
        public static readonly string DEFAULT_QUEST_BOOK_PATH = Application.persistentDataPath + "/questbook.json";
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
    }
}


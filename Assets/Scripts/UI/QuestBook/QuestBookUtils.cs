using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookUtils
    {
        public static string DEFAULT_QUEST_BOOK_PATH => Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), "questbook.json");
        public const string MAIN_QUEST_BOOK_NAME = "_main";
        public const string LIBRARY_DATA_PATH = "library_data.bin";
        public const string QUESTBOOK_DATA_PATH = "questbook_data.bin";
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
        public const bool SHOW_ALL_COMPLETED = false;
    }
}


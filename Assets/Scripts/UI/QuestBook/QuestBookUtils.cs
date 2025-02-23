using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookUtils
    {
        public static string DEFAULT_QUEST_BOOK_PATH => Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), "questbook.json");
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
        public const bool SHOW_ALL_COMPLETED = false;
    }
}


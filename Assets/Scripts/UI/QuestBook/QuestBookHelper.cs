using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public static class QuestBookHelper 
    {
        private static string nodeObjectPrefabPath = "UI/Quest/QuestBookNode";
        private static string editModePrefabPath = "UI/Quest/EditModeElements";
        private static string lineModePrefabPath = "UI/Quest/Line";
        private static string defaultQuestBookPath = Application.persistentDataPath + "/questbook.json";
        private static string selectorPrefabPath = "UI/Quest/QuestBookSelector";
        private static string bookTitlePagePrefabPath = "UI/Quest/BookTitlePage";
        private static string questBookPrefabPath = "UI/Quest/QuestBook";
        private static string questBookChapterPrefabPath = "UI/Quest/PageChapter";
        private static string editChapterPopUpPrefabPath = "UI/Quest/EditChapterPopUp";
        private static string taskContentPrefabPath = "UI/Quest/QuestBookTaskContent";
        private static string questBookSpritePath = "UI/Quest/EditQuestBook/Sprites";
        public static string NodeObjectPrefabPath { get => nodeObjectPrefabPath;}
        public static string EditModePrefabPath { get => editModePrefabPath;}
        public static string LineModePrefabPath { get => lineModePrefabPath;}
        public static string DefaultQuestBookPath {get => defaultQuestBookPath;}
        public static string SelectorPrefabPath { get => selectorPrefabPath;}
        public static string BookTitlePagePrefabPath { get => bookTitlePagePrefabPath;}
        public static string QuestBookPrefabPath { get => questBookPrefabPath;}
        public static string QuestBookChapterPrefabPath { get => questBookChapterPrefabPath;}
        public static string EditChapterPopUpPrefabPath { get => editChapterPopUpPrefabPath;}
        public static string TaskContentPrefabPath { get => taskContentPrefabPath;}
        public static string QuestBookSpritePath { get => questBookSpritePath;}
        public static bool EditMode { get => editMode; set => editMode = value; }
        private static bool editMode = false;
    }
}


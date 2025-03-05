using System.IO;
using DevTools.Structures;
using DevTools.Upgrades;
using Newtonsoft.Json;
using UI;
using UI.QuestBook;
using UI.QuestBook.Data;
using UnityEngine;

namespace DevTools.QuestBook
{
    public class DevToolQuestBookListUI : DevToolListUI
    {
        [SerializeField] private DevToolQuestBookListElement mListElementPrefab;
        [SerializeField] private QuestBookSelectorUI questBookSelectorUIPrefab;
        [SerializeField] private DevToolNewQuestBookPopUp devToolNewQuestBookPopUpPrefab;
        protected override void OnAddButtonClick()
        {
            DevToolNewQuestBookPopUp popUp = Instantiate(devToolNewQuestBookPopUpPrefab,transform,false);
            popUp.Initialize(AddNewQuestBookLibrary);
        }

        private void AddNewQuestBookLibrary(string libName)
        {
            string path = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook),libName);
            if (Directory.Exists(path) && libName != QuestBookUtils.MAIN_QUEST_BOOK_NAME)
            {
                Directory.Delete(path, true);
            }
            Directory.CreateDirectory(path);
            string libDataPath = Path.Combine(path, QuestBookUtils.LIBRARY_DATA_PATH);
            QuestBookLibraryData questBookLibraryData = QuestBookFactory.GetDefaultLibraryData();
            GlobalHelper.SerializeCompressedJson(questBookLibraryData, libDataPath);
            foreach (QuestBookSelectorData selectorData in questBookLibraryData.QuestBookDataList)
            {
                string questBookPath = Path.Combine(path, selectorData.Id);
                Directory.CreateDirectory(questBookPath);
                QuestBookData questBookData = QuestBookFactory.GetDefaultQuestBookData();
                string questBookDataPath = Path.Combine(questBookPath, QuestBookUtils.QUESTBOOK_DATA_PATH);
                GlobalHelper.SerializeCompressedJson(questBookData, questBookDataPath);
            }
            DisplayList();
        }

        public override void DisplayList()
        {
            GlobalHelper.deleteAllChildren(mList.transform);
            string path = DevToolUtils.GetDevToolPath(DevTool.QuestBook);
            string[] directories = Directory.GetDirectories(path);
            
            foreach (string directory in directories)
            {
                if (directory.Contains(".meta")) continue;
                DevToolQuestBookListElement devToolUpgradeListElement = Instantiate(mListElementPrefab,mList.transform);
                devToolUpgradeListElement.Display(directory,OnPathSelect);
            }
        }

        private void OnPathSelect(string path)
        {
            QuestBookSelectorUI questBookSelectorUI = Instantiate(questBookSelectorUIPrefab);
            string libPath = Path.Combine(path, QuestBookUtils.LIBRARY_DATA_PATH);
            QuestBookLibraryData questBookLibraryData = GlobalHelper.DeserializeCompressedJson<QuestBookLibraryData>(libPath);
            if (questBookLibraryData == null)
            {
                Debug.LogError($"Invalid lib data at path '{libPath}'");
                return;
            }
            questBookSelectorUI.Initialize(questBookLibraryData,path);
            CanvasController.Instance.DisplayObject(questBookSelectorUI.gameObject);
        }
    }
}

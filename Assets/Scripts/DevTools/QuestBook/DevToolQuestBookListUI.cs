using System.IO;
using DevTools.Structures;
using DevTools.Upgrades;
using UI;
using UI.QuestBook;
using UnityEngine;

namespace DevTools.QuestBook
{
    public class DevToolQuestBookListUI : DevToolListUI
    {
        [SerializeField] private DevToolQuestBookListElement mListElementPrefab;
        [SerializeField] private QuestBookSelectorUI questBookSelectorUIPrefab;
        protected override void OnAddButtonClick()
        {
            // No Add
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
            questBookSelectorUI.Initialize(path);
            CanvasController.Instance.DisplayObject(questBookSelectorUI.gameObject);
        }
    }
}

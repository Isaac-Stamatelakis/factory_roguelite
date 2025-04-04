using System;
using System.Collections.Generic;
using System.IO;
using DevTools;
using Player.Controls;
using PlayerModule.KeyPress;
using UI.QuestBook.Data;
using UnityEngine;

namespace UI.QuestBook
{
    public class QuestBookUIManager : MonoBehaviour
    {
        [SerializeField] private QuestBookSelectorUI selectorPrefab;
        public void Initialize(string questBookName)
        {
            GlobalHelper.DeleteAllChildren(transform);
            string path = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), questBookName);
            string libPath = Path.Combine(path, QuestBookUtils.LIBRARY_DATA_PATH);
            QuestBookLibraryData questBookLibraryData = GlobalHelper.DeserializeCompressedJson<QuestBookLibraryData>(libPath);
            if (questBookLibraryData == null)
            {
                Debug.LogError($"Invalid lib data at path '{libPath}'");
                return;
            }
            
            QuestBookSelectorUI selectorUI = Instantiate(selectorPrefab, transform, false);
            selectorUI.Initialize(questBookLibraryData,path);
            selectorUI.gameObject.SetActive(false);
        }

       
        void Update()
        {
            if (ControlUtils.GetControlKey(PlayerControl.OpenQuestBook) && !PlayerKeyPressUtils.BlockKeyInput && transform.childCount > 0)
            {
                DisplayQuestBook();
            }
        }

        public void DisplayQuestBook()
        {
            Transform child = transform.GetChild(0);
            child.gameObject.SetActive(true);
            CanvasController.Instance.DisplayObject(child.gameObject,keyCodes: new List<KeyCode> { KeyCode.L}, hideParent:false, originalParent:transform);
        }
        
    }
}

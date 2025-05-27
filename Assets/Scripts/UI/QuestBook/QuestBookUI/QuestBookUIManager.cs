using System;
using System.Collections.Generic;
using System.IO;
using DevTools;
using Newtonsoft.Json;
using Player;
using Player.Controls;
using PlayerModule.KeyPress;
using UI.QuestBook.Data;
using UnityEngine;

namespace UI.QuestBook
{
    public class QuestBookUIManager : MonoBehaviour
    {
        [SerializeField] private QuestBookSelectorUI selectorPrefab;
        public void Initialize(string questBookName, PlayerScript playerScript)
        {
            var miscKeys = playerScript.InputActions.MiscKeys;
            miscKeys.QuestBook.performed += _ => DisplayQuestBook();
            GlobalHelper.DeleteAllChildren(transform);
            string path = Path.Combine(DevToolUtils.GetDevToolPath(DevTool.QuestBook), questBookName);
            string libPath = Path.Combine(path, QuestBookUtils.LIBRARY_DATA_PATH);
            string json =  File.ReadAllText(libPath);
            QuestBookLibraryData questBookLibraryData = JsonConvert.DeserializeObject<QuestBookLibraryData>(json);
            if (questBookLibraryData == null)
            {
                Debug.LogError($"Invalid lib data at path '{libPath}'");
                return;
            }
            
            QuestBookSelectorUI selectorUI = Instantiate(selectorPrefab, transform, false);
            selectorUI.Initialize(questBookLibraryData,path);
            selectorUI.gameObject.SetActive(false);
        }
        
        public void DisplayQuestBook()
        {
            Transform child = transform.GetChild(0);
            child.gameObject.SetActive(true);
            CanvasController.Instance.DisplayObject(child.gameObject,keyCodes: PlayerControl.OpenQuestBook,
                hideParent:false, 
                originalParent:transform
            );
        }
        
    }
}

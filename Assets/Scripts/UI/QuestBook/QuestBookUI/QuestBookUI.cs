using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data;
using UI.QuestBook.Data.Node;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WorldModule;

namespace UI.QuestBook {

    public enum QuestBookUIMode {
        View,
        EditConnection
    }
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] public UIAssetManager AssetManager;
        [SerializeField] private QuestBookPageUI pageUI;
        [SerializeField] private VerticalLayoutGroup mChapterList;
        [SerializeField] private Button addChapter;
        [SerializeField] private Button backButton;
        [SerializeField] private QuestPageChapterButton chapterButtonPrefab;
        private QuestBookData questBookData;

        private string libraryPath;
        private string questBookPath;
        private string questBookId;
        private int currentIndex = -1;
        
        public void Initialize(QuestBookData questBookData, string libraryPath, string questBookId) {
            this.backButton.onClick.AddListener(BackButtonPress);
            
            this.libraryPath = libraryPath;
            this.questBookData = questBookData;
            this.questBookPath = Path.Combine(libraryPath, questBookId);
            this.questBookId = questBookId;
            AssetManager.load();
            
            if (SceneManager.GetActiveScene().name != DevToolUtils.SCENE_NAME) {
                addChapter.gameObject.SetActive(false);
            } else {
                addChapter.onClick.AddListener(() => {
                    List<string> ids = new List<string>();
                    foreach (var data in questBookData.PageDataList)
                    {
                        ids.Add(data.Id);
                    }
                    string id = GlobalHelper.GenerateHash(ids);
                    QuestBookFactory.SerializedQuestBookNodeData(Path.Combine(libraryPath,questBookId),id,new List<QuestBookNodeData>());
                    
                    questBookData.PageDataList.Add(new QuestBookPageData("New Page", id));
                    LoadPageChapters();
                });
            }
            LoadPageChapters();
            DisplayPageIndex(0);
        }

        public void LoadPageChapters() {
            for (int i = 0; i < mChapterList.transform.childCount; i++) {
                GameObject.Destroy(mChapterList.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < questBookData.PageDataList.Count; i++) {
                QuestPageChapterButton chapterButton = GameObject.Instantiate(chapterButtonPrefab, mChapterList.transform, false);
                chapterButton.Initialize(this,questBookData.PageDataList[i],questBookData, i,questBookPath);
            }
        }

        private void BackButtonPress() {
            QuestBookSelectorUI selectorUI = AssetManager.cloneElement<QuestBookSelectorUI>("TITLE");
            selectorUI.transform.SetParent(transform.parent,false);
            string libraryDataPath = Path.Combine(libraryPath, QuestBookUtils.LIBRARY_DATA_PATH);
            QuestBookLibraryData libraryData = GlobalHelper.DeserializeCompressedJson<QuestBookLibraryData>(libraryDataPath);
            selectorUI.Initialize(libraryData,libraryPath);
            GameObject.Destroy(gameObject);
            
        }
        
        public void DisplayPageIndex(int index)
        {
            if (index < 0 || index >= questBookData.PageDataList.Count || currentIndex == index) {
                return;
            }
            currentIndex = index;
            DisplayPage(questBookData.PageDataList[index]);
        }

        private void DisplayPage(QuestBookPageData page)
        {
            List<QuestBookNodeData> questBookNodeData = QuestBookFactory.GetQuestBookPageNodeData(questBookPath, page.Id);
            List<QuestBookTaskData> taskDataList;
            string playerPageDataPath = null;
            if (DevToolUtils.OnDevToolScene)
            {
                taskDataList = new List<QuestBookTaskData>();
            }
            else
            {
                string questBookPath = Path.Combine(WorldLoadUtils.GetMainPath(WorldManager.getInstance().GetWorldName()), QuestBookUtils.WORLD_QUEST_FOLDER_PATH, questBookId);
                playerPageDataPath = Path.Combine(questBookPath, page.Id) + ".bin";
                taskDataList = GlobalHelper.DeserializeCompressedJson<List<QuestBookTaskData>>(playerPageDataPath);
            }
            QuestBookPage questBookPage = QuestBookFactory.GetQuestBookPage(questBookNodeData, taskDataList);
            pageUI.Initialize(questBookPage,questBookData,page, this,questBookPath,playerPageDataPath);
        }
        
        public void OnDestroy()
        {
            if (DevToolUtils.OnDevToolScene)
            {
                GlobalHelper.SerializeCompressedJson(questBookData,Path.Combine(questBookPath,QuestBookUtils.QUESTBOOK_DATA_PATH));
            }
            else
            {
                
            }
            
        }
    }
    
}


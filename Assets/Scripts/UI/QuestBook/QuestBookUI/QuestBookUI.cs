using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using Newtonsoft.Json;
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
        private Dictionary<int, QuestBookNode> idNodeDictionary;
        
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
            InitializeNodeDictionary();
            LoadPageChapters();
            DisplayPageIndex(0);
        }

        private void InitializeNodeDictionary()
        {
            idNodeDictionary = new Dictionary<int, QuestBookNode>();
            foreach (var questBookPageData in questBookData.PageDataList)
            {
                UnOrderedPageData unOrderedPageData = GetNodesFromQuestBookPageData(questBookPageData);
                foreach (QuestBookNodeData questBookNodeData in unOrderedPageData.QuestBookNodeDataList)
                {
                    idNodeDictionary[questBookNodeData.Id] = new QuestBookNode(questBookNodeData, null);
                }

                foreach (QuestBookTaskData questBookTaskData in unOrderedPageData.TaskDataList)
                {
                    if (!idNodeDictionary.TryGetValue(questBookTaskData.Id, out var value)) continue;
                    value.TaskData = questBookTaskData;
                }

                foreach (var (id, questBookNodeData) in idNodeDictionary)
                {
                    questBookNodeData.TaskData ??= new QuestBookTaskData(false, new QuestBookRewardClaimStatus(), id);
                }
            }
            pageUI.SetIdDictionary(idNodeDictionary);
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
            string json = File.ReadAllText(libraryDataPath);
            QuestBookLibraryData libraryData = JsonConvert.DeserializeObject<QuestBookLibraryData>(json);
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
            UnOrderedPageData unOrderedPageData = GetNodesFromQuestBookPageData(page);
            QuestBookPage questBookPage = QuestBookFactory.GetQuestBookPage(unOrderedPageData.QuestBookNodeDataList, unOrderedPageData.TaskDataList);
            foreach (QuestBookNode questBookNode in questBookPage.Nodes)
            {
                idNodeDictionary[questBookNode.Id] = questBookNode;
            }
            pageUI.Initialize(questBookPage,questBookData,page, this,questBookPath,unOrderedPageData.PlayerPageDataPath);
        }

        private UnOrderedPageData GetNodesFromQuestBookPageData(QuestBookPageData page)
        {
            UnOrderedPageData unOrderedPageData = new UnOrderedPageData
            {
                QuestBookNodeDataList = QuestBookFactory.GetQuestBookPageNodeData(questBookPath, page.Id)
            };
            if (DevToolUtils.OnDevToolScene)
            {
                unOrderedPageData.TaskDataList = new List<QuestBookTaskData>();
            }
            else
            {
                string playerQuestBookPath = Path.Combine(WorldLoadUtils.GetMainPath(WorldManager.GetInstance().GetWorldName()), QuestBookUtils.WORLD_QUEST_FOLDER_PATH, questBookId);
                string playerPageDataPath = Path.Combine(playerQuestBookPath, page.Id) + ".json";
                string json = File.ReadAllText(playerPageDataPath);
                List<QuestBookTaskData> taskDataList = JsonConvert.DeserializeObject<List<QuestBookTaskData>>(json);
                unOrderedPageData.TaskDataList = taskDataList;
                unOrderedPageData.PlayerPageDataPath = playerPageDataPath;
            }

            return unOrderedPageData;
        }
        
        public void OnDestroy()
        {
            if (!DevToolUtils.OnDevToolScene) return;
            string json = JsonConvert.SerializeObject(questBookData);
            string path = Path.Combine(questBookPath, QuestBookUtils.QUESTBOOK_DATA_PATH);
            File.WriteAllText(path,json);
        }

        private struct UnOrderedPageData
        {
            public List<QuestBookNodeData> QuestBookNodeDataList;
            public List<QuestBookTaskData> TaskDataList;
            public string PlayerPageDataPath;
        }
    }
    
}


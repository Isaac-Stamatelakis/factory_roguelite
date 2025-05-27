using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DevTools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using Newtonsoft.Json;
using UI.QuestBook.Data;
using UnityEngine.SceneManagement;

namespace UI.QuestBook {
    public class QuestBookSelectorUI : MonoBehaviour
    {
        [SerializeField] public UIAssetManager AssetManager;
        [SerializeField] private SpriteKey[] spriteKeys;
        public SpriteKey[] SpriteKeys => spriteKeys;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image image;
        [SerializeField] private HorizontalLayoutGroup layoutGroup;
        [SerializeField] private Button addButton;
        [SerializeField] private QuestBookPreview questBookPreviewPrefab;
        [SerializeField] private GameObject emptyFillPrefab;
        private QuestBookLibraryData library;
        private int PageCount {get => Mathf.CeilToInt(library.QuestBookDataList.Count/((float)BooksPerPage));}
        public QuestBookLibraryData LibraryData { get => library; set => library = value; }
        private Dictionary<QuestBookTitleSpritePath, Sprite> spriteDict;
        private int BooksPerPage = 3;

        private int page = 0;
        private string path;

        public void Initialize(QuestBookLibraryData libraryData, string path)
        {
            if (!ItemRegistry.IsLoaded) {
                StartCoroutine(ItemRegistry.LoadItems());
            }
            this.library = libraryData;
            this.path = path;
            
            AssetManager.load();
            leftButton.onClick.AddListener(LeftButtonClick);
            rightButton.onClick.AddListener(RightButtonClick);
            spriteDict = new Dictionary<QuestBookTitleSpritePath, Sprite>();
            foreach (SpriteKey spriteKey in spriteKeys) {
                spriteDict[spriteKey.Key] = spriteKey.Sprite;
            }
            Display();
            addButton.onClick.AddListener(AddNewQuestBook);
            addButton.gameObject.SetActive(SceneManager.GetActiveScene().name == DevToolUtils.SCENE_NAME);
        }

        private void LeftButtonClick() {
            if (page <= 0) {
                return;
            }
            page --;
            Display();
        }

        private void AddNewQuestBook()
        {
            string id = GetNewId();
            QuestBookSelectorData selectorData = new QuestBookSelectorData("New Quest Book", QuestBookTitleSpritePath.Stars, id);
            string questBookPath = Path.Combine(path, id);
            Directory.CreateDirectory(questBookPath);
            QuestBookData questBookData = QuestBookFactory.GetDefaultQuestBookData();
            string dataPath = Path.Combine(questBookPath, QuestBookUtils.QUESTBOOK_DATA_PATH);
            string json = JsonConvert.SerializeObject(questBookData);
            File.WriteAllText(dataPath, json);
            library.QuestBookDataList.Add(selectorData);
            Display();
        }

        private string GetNewId()
        {
            const int ATTEMPTS = 64;
            for (int i = 0; i < ATTEMPTS; i++)
            {
                string hash = GlobalHelper.GenerateHash();
                bool conflict = false;
                foreach (var data in library.QuestBookDataList)
                {
                    if (data.Id == hash)
                    {
                        conflict = true;
                        break;
                    }
                }

                if (!conflict) return hash;
            }

            return null;
        }

        private void RightButtonClick() {
            if (page >= PageCount) {
                return;
            }
            page ++;
            Display();

        }

        public Sprite GetSprite(QuestBookTitleSpritePath key)
        {
            return spriteDict.GetValueOrDefault(key);
        }

        public void Display() {
            for (int i = 0; i < layoutGroup.transform.childCount; i++) {
                GameObject.Destroy(layoutGroup.transform.GetChild(i).gameObject);
            }
            if (page == 0) {
                leftButton.gameObject.SetActive(false);
            } else {
                leftButton.gameObject.SetActive(true);
            }
            if (page == PageCount-1) {
                rightButton.gameObject.SetActive(false);
            } else {
                rightButton.gameObject.SetActive(true);
            }
            for (int i = 0; i < BooksPerPage; i++) {
                int index = page*BooksPerPage+i;
                if (index >= library.QuestBookDataList.Count) {
                    Instantiate(emptyFillPrefab, layoutGroup.transform,false);
                    continue;
                }
                QuestBookPreview bookPreview = GameObject.Instantiate(questBookPreviewPrefab, layoutGroup.transform, false);
                bookPreview.Initialize(index,this,library,path);
            }
        }

        public void OnDestroy()
        {
            if (!DevToolUtils.OnDevToolScene) return;
            string libPath = Path.Combine(path, QuestBookUtils.LIBRARY_DATA_PATH);
            string json = JsonConvert.SerializeObject(library);
            File.WriteAllText(libPath, json);
        }
    }
    [System.Serializable]
    public struct SpriteKey {
        public QuestBookTitleSpritePath Key;
        public Sprite Sprite;
    }
}


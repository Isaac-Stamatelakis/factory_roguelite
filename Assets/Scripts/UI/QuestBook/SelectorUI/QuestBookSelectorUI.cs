using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;

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
        private QuestBookLibrary library;
        private int PageCount {get => Mathf.CeilToInt(library.QuestBooks.Count/((float)BooksPerPage));}
        public QuestBookLibrary Library { get => library; set => library = value; }
        private Dictionary<string, Sprite> spriteDict;
        private int BooksPerPage = 3;

        private int page = 0;

        public void Initialize(string path)
        {
            AssetManager.load();
            leftButton.onClick.AddListener(LeftButtonClick);
            rightButton.onClick.AddListener(RightButtonClick);
        }
        public void Initialize(QuestBookLibrary library) {
            if (!ItemRegistry.IsLoaded) {
                StartCoroutine(ItemRegistry.LoadItems());
            }
            this.library = library;
            spriteDict = new Dictionary<string, Sprite>();
            foreach (SpriteKey spriteKey in spriteKeys) {
                spriteDict[spriteKey.Key] = spriteKey.Sprite;
            }
            AssetManager.load();
            leftButton.onClick.AddListener(LeftButtonClick);
            rightButton.onClick.AddListener(RightButtonClick);
            Display();
            addButton.onClick.AddListener(() => {
                library.QuestBooks.Add(new QuestBook(
                    new List<QuestBookPage>(),
                    "New Book",
                    ""
                ));
                Display();
            });
            addButton.gameObject.SetActive(QuestBookUtils.EditMode);
            
        }

        private void LeftButtonClick() {
            if (page <= 0) {
                return;
            }
            page --;
            Display();
        }

        private void RightButtonClick() {
            if (page >= PageCount) {
                return;
            }
            page ++;
            Display();

        }

        public Sprite GetSprite(string key)
        {
            if (key == null) return null;
            return spriteDict.TryGetValue(key, out var sprite) ? sprite : null;
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
                if (index >= library.QuestBooks.Count) {
                    Instantiate(emptyFillPrefab, layoutGroup.transform,false);
                    continue;
                }
                QuestBookPreview bookPreview = GameObject.Instantiate(questBookPreviewPrefab, layoutGroup.transform, false);
                bookPreview.init(index,this,library);
            }
        }

        
    }
    [System.Serializable]
    public struct SpriteKey {
        public string Key;
        public Sprite Sprite;
    }
}


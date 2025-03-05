using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data;

namespace UI.QuestBook {
    public class EditQuestBookUI : MonoBehaviour
    {
        [SerializeField] private SpriteKey[] spriteKeys;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TMP_InputField editTitle;
        [SerializeField] private TMP_InputField confirmDelete;
        [SerializeField] private Button backButton;
        [SerializeField] private GridLayoutGroup spriteList;
        [SerializeField] private TextMeshProUGUI confirmDeleteText;
        [SerializeField] private TextMeshProUGUI confirmDeleteHint;
        [SerializeField] private EditQuestBookSpriteElementUI editQuestBookSpriteElementUIPrefab;
        private EditQuestBookSpriteElementUI[] listElements;
        private QuestBookSelectorData questBookSelectorData {get => library.QuestBookDataList[index];}
        private int index;
        private float deletionFailTime = 1f;
        private QuestBookLibraryData library;
        private QuestBookSelectorUI selectorUI;

        public void Initialize(QuestBookSelectorUI selectorUI, QuestBookLibraryData library, int index, string libraryPath) {
            this.index = index;
            this.library = library;
            this.selectorUI = selectorUI;

            editTitle.text = questBookSelectorData.Title;


            displaySprites();
            backButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
            leftButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index-1,library.QuestBookDataList.Count);
                (library.QuestBookDataList[index], library.QuestBookDataList[newIndex]) = (library.QuestBookDataList[newIndex], library.QuestBookDataList[index]);
                index = newIndex;
                selectorUI.Display();
            });
            rightButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index+1,library.QuestBookDataList.Count);
                (library.QuestBookDataList[index], library.QuestBookDataList[newIndex]) = (library.QuestBookDataList[newIndex], library.QuestBookDataList[index]);
                index = newIndex;
                selectorUI.Display();
            });
            deleteButton.onClick.AddListener(() => {
                if (confirmDelete.text == questBookSelectorData.Title)
                {
                    var toRemove = questBookSelectorData;
                    library.QuestBookDataList.RemoveAt(index);
                    string questBookPath = Path.Combine(libraryPath, toRemove.Id);
                    Directory.Delete(questBookPath, true);
                    selectorUI.Display();
                    GameObject.Destroy(gameObject);
                    return;
                }
                deletionFailTime = 0f;
            });
            editTitle.onValueChanged.AddListener((string value) => {
                questBookSelectorData.Title = value;
                selectorUI.Display();
            });
        }

        public void Update() {
            if (deletionFailTime < 1f) {
                confirmDeleteHint.color = Color.red;
                confirmDeleteText.color = Color.red;
            } else if (deletionFailTime < 1.2f) {
                confirmDeleteHint.color = new Color(50f/255f,50f/255f,50f/255f,128f/255f);
                confirmDeleteText.color = Color.black;
            }
            deletionFailTime += Time.deltaTime;
            
        }

        public void displaySprites() {
            SpriteKey[] spriteKeys = selectorUI.SpriteKeys;
            listElements = new EditQuestBookSpriteElementUI[spriteKeys.Length];
            for (int i = 0; i < spriteKeys.Length; i++) {
                SpriteKey spriteKey = spriteKeys[i];
                EditQuestBookSpriteElementUI spriteElement = GameObject.Instantiate(editQuestBookSpriteElementUIPrefab);
                spriteElement.transform.SetParent(spriteList.transform);
                spriteElement.init(questBookSelectorData,this,spriteKey.Sprite,spriteKey.Key);
                listElements[i] = spriteElement;
            }
        }
        public void loadSpritePanelColors() {
            selectorUI.Display();
            foreach (EditQuestBookSpriteElementUI element in listElements) {
                element.setPanelColor();
            }
        }
    }
}


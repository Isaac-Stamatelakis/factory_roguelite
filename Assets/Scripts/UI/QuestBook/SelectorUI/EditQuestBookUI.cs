using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        private QuestBook questBook {get => library.QuestBooks[index];}
        private int index;
        private float deletionFailTime = 1f;
        private QuestBookLibrary library;
        private QuestBookSelectorUI selectorUI;

        public void init(QuestBookSelectorUI selectorUI, QuestBookLibrary library, int index) {
            this.index = index;
            this.library = library;
            this.selectorUI = selectorUI;

            editTitle.text = questBook.Title;


            displaySprites();
            backButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
            leftButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index-1,library.QuestBooks.Count);
                QuestBook swap = library.QuestBooks[index];
                library.QuestBooks[index] = library.QuestBooks[newIndex];
                library.QuestBooks[newIndex] = swap;
                index = newIndex;
                selectorUI.display();
            });
            rightButton.onClick.AddListener(() => {
                int newIndex = Global.modInt(index+1,library.QuestBooks.Count);
                QuestBook swap = library.QuestBooks[index];
                library.QuestBooks[index] = library.QuestBooks[newIndex];
                library.QuestBooks[newIndex] = swap;
                index = newIndex;
                selectorUI.display();
            });
            deleteButton.onClick.AddListener(() => {
                if (confirmDelete.text == questBook.Title) {
                    library.QuestBooks.RemoveAt(index);
                    selectorUI.display();
                    GameObject.Destroy(gameObject);
                    return;
                }
                deletionFailTime = 0f;
            });
            editTitle.onValueChanged.AddListener((string value) => {
                questBook.Title = value;
                selectorUI.display();
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
                spriteElement.init(questBook,this,spriteKey.Sprite,spriteKey.Key);
                listElements[i] = spriteElement;
            }
        }
        public void loadSpritePanelColors() {
            selectorUI.display();
            foreach (EditQuestBookSpriteElementUI element in listElements) {
                element.setPanelColor();
            }
        }
    }
}


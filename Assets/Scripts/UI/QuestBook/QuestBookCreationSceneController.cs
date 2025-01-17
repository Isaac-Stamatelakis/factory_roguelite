using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class QuestBookCreationSceneController : MonoBehaviour
    {
        [SerializeField] public Transform canvasTransform;
        [SerializeField] private bool editMode;
        [SerializeField] private QuestBookSelectorUI questBookSelectorUIPrefab;
        [SerializeField] private Button backButtonPrefab;
        private QuestBookLibrary library;
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(Application.persistentDataPath);
            string json = null;
            if (File.Exists(QuestBookHelper.DEFAULT_QUEST_BOOK_PATH)) {
                json = File.ReadAllText(QuestBookHelper.DEFAULT_QUEST_BOOK_PATH);
            }
            try
            {
                library = QuestBookLibraryFactory.Deseralize(json);
            } catch (Exception e)  {
                Debug.LogError(e);
                library = null;
            }
            
            if (library == null) {
                List<QuestBook> books = new List<QuestBook>{
                    new QuestBook(
                        new List<QuestBookPage>{
                            new QuestBookPage("Chapter0", new List<QuestBookNode>())
                        },
                        "A Dummies Guide to Portal Creation",
                        "Sprites/QuestBook/bg5"
                    ),
                    new QuestBook(
                        new List<QuestBookPage>{
                            new QuestBookPage("Chapter0", new List<QuestBookNode>())
                        },
                        "Navigating Advanced Technology",
                        "Sprites/QuestBook/bg6"
                    ),
                    new QuestBook(
                        new List<QuestBookPage>{
                            new QuestBookPage("Chapter0", new List<QuestBookNode>())
                        },
                        "Alien Artifact Research Notes",
                        "Sprites/QuestBook/cb1"
                    ),
                    new QuestBook(
                        new List<QuestBookPage>{
                            new QuestBookPage("Chapter0", new List<QuestBookNode>())
                        },
                        "Home Improvement",
                        "Sprites/QuestBook/cb1"
                    )
                };
                library = new QuestBookLibrary(books);
            }
            
            GameObject container = new GameObject();
            RectTransform rectTransform = container.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0,0);
            rectTransform.anchorMax = new Vector2(1,1);
            container.name = "QuestContainer";
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            container.transform.SetParent(canvas.transform,false);
            rectTransform.sizeDelta = new Vector2(0,0);

            QuestBookSelectorUI questBookSelectorUI = GameObject.Instantiate(questBookSelectorUIPrefab);
            questBookSelectorUI.Initialize(library);
            
            questBookSelectorUI.transform.SetParent(container.transform,false);
            Button backButton = GameObject.Instantiate(backButtonPrefab);
            backButton.transform.SetParent(container.transform,false);
            backButton.transform.localPosition = new Vector2(-798,482);
            
            backButton.onClick.AddListener(() => {
                GameObject.Destroy(container);
                GameObject.Destroy(gameObject);
            });
        }

        void OnDestroy() {
            string json = QuestBookLibraryFactory.Serialize(library);
            File.WriteAllText(QuestBookHelper.DEFAULT_QUEST_BOOK_PATH,json);
        }
        void Awake()
        {
            QuestBookHelper.EditMode = editMode;
        }
    }
}


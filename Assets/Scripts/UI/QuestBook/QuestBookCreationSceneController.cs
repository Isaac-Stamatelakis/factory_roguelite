using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System;

namespace UI.QuestBook {
    public class QuestBookCreationSceneController : MonoBehaviour
    {
        [SerializeField] public Transform canvasTransform;
        [SerializeField] private bool editMode;
        [SerializeField] private QuestBookSelectorUI questBookSelectorUIPrefab;
        private QuestBookLibrary library;
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log(Application.persistentDataPath);
            string json = "null";
            if (File.Exists(QuestBookHelper.DefaultQuestBookPath)) {
                json = File.ReadAllText(QuestBookHelper.DefaultQuestBookPath);
            }
            try {
                library = QuestBookLibraryFactory.deseralize(json);
            } catch (Exception e)  {
                Debug.LogWarning(e);
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
            QuestBookSelectorUI questBookSelectorUI = GameObject.Instantiate(questBookSelectorUIPrefab);
            questBookSelectorUI.init(library);
            questBookSelectorUI.transform.SetParent(canvasTransform,false);
        }

        void OnDestroy() {
            string json = QuestBookLibraryFactory.seralize(library);
            File.WriteAllText(QuestBookHelper.DefaultQuestBookPath,json);
        }
        void Awake()
        {
            QuestBookHelper.EditMode = editMode;
        }
    }
}


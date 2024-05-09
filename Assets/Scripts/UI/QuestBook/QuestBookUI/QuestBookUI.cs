using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {

    public enum QuestBookUIMode {
        View,
        EditConnection
    }
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] private QuestBookPageUI pageUI;
        [SerializeField] private GridLayoutGroup chapterGridGroup;
        [SerializeField] private Button addChapter;
        [SerializeField] private Button backButton;
        [SerializeField] private QuestPageChapterButton chapterButtonPrefab;
        public QuestBook QuestBook { get => questBook; set => questBook = value; }
        public QuestBookPage CurrentPage { get => currentPage; set => currentPage = value; }
        public QuestBookLibrary Library { get => library; set => library = value; }
        private QuestBook questBook;
        private GameObject selectorObject;
        private QuestBookPage currentPage;
        private QuestBookLibrary library;
        public void init(QuestBook questBook, QuestBookLibrary library, GameObject selectorObject) {
            this.questBook = questBook;
            this.selectorObject = selectorObject;
            this.library = library;
            this.backButton.onClick.AddListener(backButtonPress);
            
            if (!QuestBookHelper.EditMode) {
                addChapter.gameObject.SetActive(false);
            } else {
                addChapter.onClick.AddListener(() => {
                    questBook.Pages.Add(new QuestBookPage("New Page", new List<QuestBookNode>()));
                    loadPageChapters();
                });
            }
            loadPageChapters();
            displayPageIndex(0);
        }

        public void loadPageChapters() {
            for (int i = 0; i < chapterGridGroup.transform.childCount; i++) {
                GameObject.Destroy(chapterGridGroup.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < questBook.Pages.Count; i++) {
                QuestPageChapterButton chapterButton = GameObject.Instantiate(chapterButtonPrefab);
                chapterButton.transform.SetParent(chapterGridGroup.transform,false);
                chapterButton.init(this,questBook.Pages[i],i);
            }
        }

        private void backButtonPress() {
            selectorObject.SetActive(true);
            GameObject.Destroy(gameObject);
        }
        
        public void displayPageIndex(int index) {
            if (index < 0 || index >= questBook.Pages.Count) {
                return;
            }
            displayPage(questBook.Pages[index]);
        }

        private void displayPage(QuestBookPage page) {
            currentPage = page;
            pageUI.init(page,questBook,library,this);
            pageUI.display();
        }
        public void displayCurrentPage() {
            displayPage(currentPage);
        }
    }
    
}


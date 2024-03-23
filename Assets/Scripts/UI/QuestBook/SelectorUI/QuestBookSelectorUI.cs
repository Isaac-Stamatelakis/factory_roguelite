using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class QuestBookSelectorUI : MonoBehaviour
    {
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Image image;
        [SerializeField] private GridLayoutGroup layoutGroup;
        [SerializeField] private Button addButton;
        private QuestBookLibrary library;
        private int PageCount {get => library.QuestBooks.Count/BooksPerPage;}
        public QuestBookLibrary Library { get => library; set => library = value; }

        private int BooksPerPage = 3;

        private int page = 0;
        public void init(QuestBookLibrary library) {
            this.library = library;
            leftButton.onClick.AddListener(leftButtonClick);
            rightButton.onClick.AddListener(rightButtonClick);
            display();
            addButton.onClick.AddListener(() => {
                library.QuestBooks.Add(new QuestBook(
                    new List<QuestBookPage>(),
                    "New Book",
                    ""
                ));
                display();
            });
            addButton.gameObject.SetActive(QuestBookHelper.EditMode);
        }

        private void leftButtonClick() {
            if (page <= 0) {
                return;
            }
            page --;
            display();
        }

        private void rightButtonClick() {
            if (page >= PageCount) {
                return;
            }
            page ++;
            display();

        }

        public void display() {
            for (int i = 0; i < layoutGroup.transform.childCount; i++) {
                GameObject.Destroy(layoutGroup.transform.GetChild(i).gameObject);
            }
            if (page == 0) {
                leftButton.gameObject.SetActive(false);
            } else {
                leftButton.gameObject.SetActive(true);
            }
            if (page == PageCount) {
                rightButton.gameObject.SetActive(false);
            } else {
                rightButton.gameObject.SetActive(true);
            }
            for (int i = 0; i < BooksPerPage; i++) {
                int index = page*BooksPerPage+i;
                if (index >= library.QuestBooks.Count) {
                    break;
                }
                GameObject instantiatedBookPreview = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.BookTitlePagePrefabPath));
                instantiatedBookPreview.transform.SetParent(layoutGroup.transform,false);
                QuestBookPreview bookPreview = instantiatedBookPreview.GetComponent<QuestBookPreview>();
                bookPreview.init(index,this,library);
            }
        }
    }
}


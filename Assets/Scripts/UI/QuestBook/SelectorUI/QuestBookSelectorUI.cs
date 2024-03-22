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
        private QuestBookLibrary library;
        private int PageCount {get => library.QuestBooks.Count/BooksPerPage;}
        private int BooksPerPage = 3;

        private int page = 0;
        public void init(QuestBookLibrary library) {
            this.library = library;
            leftButton.onClick.AddListener(leftButtonClick);
            rightButton.onClick.AddListener(rightButtonClick);
            display();
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

        private void display() {
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
                if (i + page * BooksPerPage >= library.QuestBooks.Count) {
                    break;
                }
                GameObject instantiatedBookPreview = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.BookTitlePagePrefabPath));
                instantiatedBookPreview.transform.SetParent(layoutGroup.transform,false);
                QuestBookPreview bookPreview = instantiatedBookPreview.GetComponent<QuestBookPreview>();
                bookPreview.init(library.QuestBooks[i],gameObject);
            }
        }
    }
}


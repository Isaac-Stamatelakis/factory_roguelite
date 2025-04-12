using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data;

namespace UI.QuestBook {
    public class EditQuestPagePopup : MonoBehaviour
    {
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image deleteButtonPanel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_InputField inputField;
        private float timeSinceLastDeletePress = 1f;
        private List<QuestBookPageData> pages;
        private QuestBookData questBookData;
        private QuestBookUI questBookUI;
        private int index;
        private string path;
        public void init(int index, QuestBookData questBookData, QuestBookUI questBookUI, string questBookPagePath) {
            this.index = index;
            this.path = questBookPagePath;
            this.questBookUI = questBookUI;
            pages = questBookData.PageDataList;
            QuestBookPageData page = pages[index];
            inputField.text = page.Title;
            inputField.onValueChanged.AddListener((string value) => {page.Title = value;questBookUI.LoadPageChapters();});
            upButton.onClick.AddListener(MoveUp);
            downButton.onClick.AddListener(MoveDown);
            backButton.onClick.AddListener(BackButtonPress);
            deleteButton.onClick.AddListener(DeleteButtonPress);
        }

        public void Update() {
            
            timeSinceLastDeletePress += Time.deltaTime;
            if (timeSinceLastDeletePress <= 1f) {
                deleteButtonPanel.color = Color.red;
            } else {
                deleteButtonPanel.color = Color.white;
            }
            
        }

        private void BackButtonPress() {
            GameObject.Destroy(gameObject);
        }
        private void MoveUp() {
            int newIndex = Global.ModInt(index-1,pages.Count);
            (pages[index], pages[newIndex]) = (pages[newIndex], pages[index]);
            index = newIndex;
            questBookUI.LoadPageChapters();
        }

        private void MoveDown() {
            int newIndex = Global.ModInt(index+1,pages.Count);
            (pages[index], pages[newIndex]) = (pages[newIndex], pages[index]);
            index = newIndex;
            questBookUI.LoadPageChapters();
        }

        private void DeleteButtonPress() {
            
            if (timeSinceLastDeletePress <= 1f) {
                pages.RemoveAt(index);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                questBookUI.LoadPageChapters();
                GameObject.Destroy(gameObject);
            }
            timeSinceLastDeletePress = 0;
            
        }
    }
}


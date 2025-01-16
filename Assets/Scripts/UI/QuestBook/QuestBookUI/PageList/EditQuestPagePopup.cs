using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        
        private List<QuestBookPage> pages;
        private QuestBookUI questBookUI;
        private int index;
        public void init(int index, List<QuestBookPage> pages, QuestBookUI questBookUI) {
            this.index = index;
            this.pages = pages;
            this.questBookUI = questBookUI;
            QuestBookPage page = pages[index];
            inputField.text = page.Title;
            inputField.onValueChanged.AddListener((string value) => {page.Title = value;questBookUI.LoadPageChapters();});
            upButton.onClick.AddListener(moveUp);
            downButton.onClick.AddListener(moveDown);
            backButton.onClick.AddListener(backButtonPress);
            deleteButton.onClick.AddListener(deleteButtonPress);
        }

        public void Update() {
            
            timeSinceLastDeletePress += Time.deltaTime;
            if (timeSinceLastDeletePress <= 1f) {
                deleteButtonPanel.color = Color.red;
            } else {
                deleteButtonPanel.color = Color.white;
            }
            
        }

        private void backButtonPress() {
            GameObject.Destroy(gameObject);
        }
        private void moveUp() {
            int newIndex = Global.modInt(index-1,pages.Count);
            QuestBookPage swap = pages[index];
            pages[index] = pages[newIndex];
            pages[newIndex] = swap;
            index = newIndex;
            questBookUI.LoadPageChapters();
        }

        private void moveDown() {
            int newIndex = Global.modInt(index+1,pages.Count);
            QuestBookPage swap = pages[index];
            pages[index] = pages[newIndex];
            pages[newIndex] = swap;
            index = newIndex;
            questBookUI.LoadPageChapters();
        }

        private void deleteButtonPress() {
            
            if (timeSinceLastDeletePress <= 1f) {
                pages.RemoveAt(index);
                questBookUI.LoadPageChapters();
                GameObject.Destroy(gameObject);
            }
            timeSinceLastDeletePress = 0;
            
        }
    }
}


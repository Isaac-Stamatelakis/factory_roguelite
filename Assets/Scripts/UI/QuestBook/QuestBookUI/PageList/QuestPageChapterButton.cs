using System.Collections;
using System.Collections.Generic;
using DevTools;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class QuestPageChapterButton : MonoBehaviour, IPointerClickHandler, ILongClickable, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private EditQuestPagePopup editQuestPagePopupPrefab;
        private int index;
        private QuestBookUI questBookUI;
        private LongClickHandler  holdClickInstance;
        private QuestBookData questBookData;
        private string path;
        public void Initialize(QuestBookUI questBookUI, QuestBookPageData page, QuestBookData questBookData, int index, string questBookPath) {
            this.index = index;
            this.path = questBookPath;
            this.questBookUI = questBookUI;
            this.questBookData = questBookData;
            text.text = page.Title;
            holdClickInstance = new LongClickHandler (this);
            
        }

        public void Update() {
            if (holdClickInstance != null) {
                holdClickInstance.checkHoldStatus();
            }
            
        }
        public void longClick()
        {
            if (SceneManager.GetActiveScene().name != DevToolUtils.SCENE_NAME) {
                return;
            }
            EditQuestPagePopup editQuestPagePopup = GameObject.Instantiate(editQuestPagePopupPrefab);
            editQuestPagePopup.init(index,questBookData,questBookUI,path);
            editQuestPagePopup.transform.SetParent(questBookUI.transform,false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                questBookUI.DisplayPageIndex(index);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            holdClickInstance.click();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            holdClickInstance.release();
        }
        
    }
}


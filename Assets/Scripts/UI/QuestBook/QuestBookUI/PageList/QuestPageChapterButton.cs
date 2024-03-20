using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class QuestPageChapterButton : MonoBehaviour, IPointerClickHandler, ILongClickable, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        private int index;
        private QuestBookUI questBookUI;
        private QuestBookPage page;
        private HoldClickInstance holdClickInstance;
        public void init(QuestBookUI questBookUI, QuestBookPage page, int index) {
            this.index = index;
            this.page = page;
            this.questBookUI = questBookUI;
            text.text = page.Title;
            holdClickInstance = new HoldClickInstance(this);
            
        }

        public void Update() {
            if (holdClickInstance != null) {
                holdClickInstance.checkHoldStatus();
            }
            
        }
        public void longClick()
        {
            if (!questBookUI.EditMode) {
                return;
            }
            GameObject instantiated = GameObject.Instantiate(Resources.Load<GameObject>(QuestBookHelper.EditChapterPopUpPrefabPath));
            EditQuestPagePopup editQuestPagePopup = instantiated.GetComponent<EditQuestPagePopup>();
            editQuestPagePopup.init(index,questBookUI.QuestBook.Pages,questBookUI);
            editQuestPagePopup.transform.SetParent(questBookUI.transform,false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                questBookUI.displayPageIndex(index);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                handleRightClick();
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

        private void handleRightClick() {
            if (!questBookUI.EditMode) {
                return;
            } 

        }
    }
}


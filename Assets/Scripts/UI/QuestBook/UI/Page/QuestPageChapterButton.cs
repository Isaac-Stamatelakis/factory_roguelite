using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UI.QuestBook {
    public class QuestPageChapterButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        private int index;
        private QuestBookUI questBookUI;
        private QuestBookPage page;
        public void init(QuestBookUI questBookUI, QuestBookPage page, int index) {
            this.index = index;
            this.page = page;
            this.questBookUI = questBookUI;
            text.text = page.Title;
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                questBookUI.displayPage(index);
            } else if (eventData.button == PointerEventData.InputButton.Right) {
                handleRightClick();
            }
        }

        private void handleRightClick() {
            if (!questBookUI.EditMode) {
                return;
            } 

        }
    }
}


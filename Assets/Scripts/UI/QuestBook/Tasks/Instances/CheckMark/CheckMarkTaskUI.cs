using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class CheckMarkTaskUI : QuestBookTaskUI<CheckMarkQuestTask>
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite[] checkMarkSprites;
        private CheckMarkQuestTask checkMarkQuestTask;
        private QuestBookTaskPageUI questBookPageUI;
        public override void init(CheckMarkQuestTask checkMarkQuestTask, QuestBookTaskPageUI questBookUI) {
            this.checkMarkQuestTask = checkMarkQuestTask;
            button.onClick.AddListener(CheckClick);
            questBookPageUI = questBookUI;
            SetImage();
        }

        private void CheckClick()
        {
            if (checkMarkQuestTask.Clicked && !QuestBookUtils.EditMode) return;
            checkMarkQuestTask.Clicked = !checkMarkQuestTask.Clicked;   
            questBookPageUI.OnTaskStatusChanged();
            SetImage();
        }

        private void SetImage() {
            int index = checkMarkQuestTask.Clicked ? 1 : 0;
            buttonImage.sprite = checkMarkSprites[index];
        }
    }
}

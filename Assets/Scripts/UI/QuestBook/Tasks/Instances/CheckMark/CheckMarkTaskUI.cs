using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class CheckMarkTaskUI : QuestBookTaskUI<CheckMarkQuestTask>
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        private Sprite[] checkMarkSprites;
        private CheckMarkQuestTask checkMarkQuestTask;
        public override void init(CheckMarkQuestTask checkMarkQuestTask, QuestBookPageUI questBookUI) {
            this.checkMarkQuestTask = checkMarkQuestTask;
            button.onClick.AddListener(checkClick);
            checkMarkSprites = Resources.LoadAll<Sprite>("Sprites/QuestBook/checkmark_sprites");
            setImage();
        }

        private void checkClick() {
            checkMarkQuestTask.Clicked = !checkMarkQuestTask.Clicked;   
            setImage();
        }

        private void setImage() {
            int index = checkMarkQuestTask.Clicked ? 1 : 0;
            buttonImage.sprite = checkMarkSprites[index];
        }
    }
}

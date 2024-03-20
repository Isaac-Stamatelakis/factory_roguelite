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
        public override void init(CheckMarkQuestTask checkMarkQuestTask) {
            this.checkMarkQuestTask = checkMarkQuestTask;
            button.onClick.AddListener(checkClick);
            checkMarkSprites = Resources.LoadAll<Sprite>("Sprites/QuestBook/checkmark_sprites");
        }

        private void checkClick() {
            checkMarkQuestTask.Clicked = !checkMarkQuestTask.Clicked;
            int index = checkMarkQuestTask.Clicked ? 0 : 1;
            buttonImage.sprite = checkMarkSprites[index];
        }
    }
}

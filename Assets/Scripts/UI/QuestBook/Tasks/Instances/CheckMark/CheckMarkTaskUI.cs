using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.QuestBook {
    public class CheckMarkTaskUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Sprite[] checkMarkSprites;
        private QuestBookTaskData taskData;
        private QuestBookTaskPageUI questBookUI;
        private void CheckClick()
        {
            if (taskData.Complete && !QuestBookUtils.EditMode) return;
            taskData.Complete = true; 
            questBookUI.OnTaskStatusChanged();
            SetImage();
        }

        private void SetImage() {
            int index = taskData.Complete ? 1 : 0;
            buttonImage.sprite = checkMarkSprites[index];
        }
        
        public void Display(QuestBookTaskPageUI questBookUI, QuestBookTaskData taskData)
        {
            button.onClick.AddListener(CheckClick);
            this.questBookUI = questBookUI;
            this.taskData = taskData;
            SetImage();
        }
    }
}

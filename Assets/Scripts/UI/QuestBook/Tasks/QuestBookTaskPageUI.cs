using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace UI.QuestBook {
    public class QuestBookTaskPageUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI taskTitle;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private GridLayoutGroup rewardsGroup;
        [SerializeField] private Button acceptRewardsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform taskContainer;
        
        public void init(QuestBookNodeContent nodeContent, QuestBookUI questBookUI) {
            this.title.text = nodeContent.Title;
            this.description.text = nodeContent.Description;
            if (nodeContent.Task == null) {
                this.taskTitle.text = "No Task";
            } else {
                this.taskTitle.text = nodeContent.Task.getTaskType().ToString().Replace("_"," ");
                GameObject questContent = QuestBookTaskUIFactory.getContent(nodeContent.Task);
                questContent.transform.SetParent(taskContainer,false);
            }
            
            backButton.onClick.AddListener(() => {questBookUI.gameObject.SetActive(true);GameObject.Destroy(gameObject);});
            
        }
    }
}


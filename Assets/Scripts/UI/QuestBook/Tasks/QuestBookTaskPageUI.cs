using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;


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
        [SerializeField] private Button editButton;
        [SerializeField] private Transform editContainer;
        [SerializeField] private TMP_InputField editTitle;
        [SerializeField] private TMP_InputField editDescription;
        [SerializeField] private TMP_Dropdown changeTaskDropDown;

        private bool editMode = false;

        private QuestBookNodeContent nodeContent;
        
        public void init(QuestBookNodeContent nodeContent, QuestBookUI questBookUI) {
            this.title.text = nodeContent.Title;
            this.description.text = nodeContent.Description;
            this.nodeContent = nodeContent;
            if (nodeContent.Task == null) {
                this.taskTitle.text = "No Task";
            } else {
                setTaskContent();
            }
            
            backButton.onClick.AddListener(() => {questBookUI.gameObject.SetActive(true);GameObject.Destroy(gameObject);});
            if (questBookUI.EditMode) {
                editButton.onClick.AddListener(goIntoEditMode);
                

            } else {
                editButton.gameObject.SetActive(false);
            }
            editContainer.gameObject.SetActive(false);
        }

        private void setTaskContent() {
            for (int i = 0; i < taskContainer.childCount; i++) {
                GameObject.Destroy(taskContainer.GetChild(i).gameObject);
            }
            this.taskTitle.text = nodeContent.Task.getTaskType().ToString().Replace("_"," ");
            GameObject questContent = QuestBookTaskUIFactory.getContent(nodeContent.Task);
            questContent.transform.SetParent(taskContainer,false);
        }
        private void goIntoEditMode() {
            editMode = !editMode;
            if (editMode) {
                editTitle.onValueChanged.RemoveAllListeners();
                editDescription.onValueChanged.RemoveAllListeners();
                changeTaskDropDown.onValueChanged.RemoveAllListeners();

                editContainer.gameObject.SetActive(true);
                editTitle.onValueChanged.AddListener((string value) => {nodeContent.Title = value;});
                editTitle.text = nodeContent.Title;
                
                editDescription.onValueChanged.AddListener((string value) => {nodeContent.Description = value;});
                editDescription.text = nodeContent.Description;

                changeTaskDropDown.ClearOptions();
                List<TMPro.TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                foreach (QuestTaskType taskType in Enum.GetValues(typeof(QuestTaskType))) {
                    options.Add(new TMPro.TMP_Dropdown.OptionData(taskType.ToString()));
                }
                changeTaskDropDown.AddOptions(options);
                changeTaskDropDown.onValueChanged.AddListener(dropDownValueChanged);
            } else {
                editContainer.gameObject.SetActive(false);
                this.title.text = nodeContent.Title;
                this.description.text = nodeContent.Description;
            }
            
        }

        private void dropDownValueChanged(int value) {
            QuestTaskType selectedTaskType = (QuestTaskType)Enum.Parse(typeof(QuestTaskType), changeTaskDropDown.options[value].text);
            if (nodeContent.Task != null && nodeContent.Task.getTaskType() == selectedTaskType) {
                return;
            }
            switch (selectedTaskType) {
                case QuestTaskType.Checkmark:
                    nodeContent.Task = new CheckMarkQuestTask();
                    break;
                case QuestTaskType.Item:
                    nodeContent.Task = new ItemQuestTask();
                    break;
                case QuestTaskType.Dimension:
                    nodeContent.Task = new VisitDimensionQuestTask();
                    break;
            }
            setTaskContent();
        }
    }
}


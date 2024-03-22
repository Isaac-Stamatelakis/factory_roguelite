using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;


namespace UI.QuestBook {
    public class QuestBookTaskPageUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup rewardsGroup;
        [SerializeField] private Button acceptRewardsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Transform taskContainer;
        [SerializeField] private Button editButton;
        [SerializeField] private Transform editContainer;
        [SerializeField] private TMP_InputField titleField;
        [SerializeField] private TMP_InputField descriptionField;
        [SerializeField] private TMP_Dropdown changeTaskDropDown;
        [SerializeField] private Button addRewardButton;

        private QuestBookNodeContent nodeContent;
        private QuestBookUI questBookUI;

        public QuestBookUI QuestBookUI { get => questBookUI; set => questBookUI = value; }
        public QuestBookNodeContent Content { get => nodeContent; set => nodeContent = value; }
        public List<int> SelectedRewards { get => selectedRewards; set => selectedRewards = value; }

        private List<int> selectedRewards = new List<int>();

        public bool RewardsSelectable {get => Content.Rewards.Count > Content.NumberOfRewards;}

        public void init(QuestBookNodeContent nodeContent, QuestBookUI questBookUI) {
            this.titleField.text = nodeContent.Title;
            this.descriptionField.text = nodeContent.Description;
            this.questBookUI = questBookUI;
            this.nodeContent = nodeContent;
            if (nodeContent.Task == null) {
                this.titleField.text = "No Task";
            } else {
                setTaskContent();
            }
            titleField.interactable = questBookUI.EditMode;
            descriptionField.interactable = questBookUI.EditMode;
            displayRewards();
            if (questBookUI.EditMode) {
                titleField.onValueChanged.AddListener((string value) => {nodeContent.Title = value;});
                descriptionField.onValueChanged.AddListener((string value) => {nodeContent.Description = value;});

                changeTaskDropDown.ClearOptions();
                List<TMPro.TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                foreach (QuestTaskType taskType in Enum.GetValues(typeof(QuestTaskType))) {
                    options.Add(new TMPro.TMP_Dropdown.OptionData(taskType.ToString()));
                }
                changeTaskDropDown.AddOptions(options);

                QuestTaskType? currentTask = nodeContent.Task == null ? null : nodeContent.Task.getTaskType();
                int currentTaskIndex = currentTask == null ? 0 : (int) currentTask;
                changeTaskDropDown.value = currentTaskIndex;
                changeTaskDropDown.onValueChanged.AddListener(dropDownValueChanged);
            }
            addRewardButton.onClick.AddListener(() => {
                Content.Rewards.Add(new SerializedItemSlot("stone",1,null));
                displayRewards();
            });
            backButton.onClick.AddListener(() => {
                questBookUI.gameObject.SetActive(true);
                GameObject.Destroy(gameObject);
            });
            editButton.onClick.AddListener(editButtonPress);
            editContainer.gameObject.SetActive(questBookUI.EditMode);

        }

        private void setTaskContent() {
            for (int i = 0; i < taskContainer.childCount; i++) {
                GameObject.Destroy(taskContainer.GetChild(i).gameObject);
            }
            this.titleField.text = nodeContent.Task.getTaskType().ToString().Replace("_"," ");
            GameObject questContent = QuestBookTaskUIFactory.getContent(nodeContent.Task, questBookUI);
            questContent.transform.SetParent(taskContainer,false);
        }
        private void editButtonPress() {

            
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

        public void addReward(int index) {
            if (selectedRewards.Count < Content.NumberOfRewards) {
                selectedRewards.Add(index);
            } else {
                selectedRewards.RemoveAt(0);
                selectedRewards.Add(index);
            }
        }

        public void removeReward(int index) {
            selectedRewards.RemoveAt(index);
        }

        public void displayRewards() {
            GlobalHelper.deleteAllChildren(rewardsGroup.transform);
            if (Content.Rewards == null) {
                Content.Rewards = new List<SerializedItemSlot>();
            }
            for (int i = 0; i < Content.Rewards.Count; i++) {
                RewardListElement rewardListElement = RewardListElement.newInstance();
                rewardListElement.init(Content.Rewards,i,this);
                rewardListElement.transform.SetParent(rewardsGroup.transform,false);
            }
        }
    }
}


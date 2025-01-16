using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;
using UI.QuestBook.Tasks.Rewards;
using UnityEngine.Serialization;


namespace UI.QuestBook {
    public class QuestBookTaskPageUI : MonoBehaviour
    {
        public UIAssetManager AssetManager;
        [SerializeField] private VerticalLayoutGroup mRewardsList;
        [FormerlySerializedAs("acceptRewardsButton")] [SerializeField] private Button mAcceptRewardsButton;
        [FormerlySerializedAs("backButton")] [SerializeField] private Button mBackButton;
        [FormerlySerializedAs("taskContainer")] [SerializeField] private Transform mTaskContainer;
        [FormerlySerializedAs("editButton")] [SerializeField] private Button mEditButton;
        [FormerlySerializedAs("titleField")] [SerializeField] private TMP_InputField mTitleField;
        [FormerlySerializedAs("descriptionField")] [SerializeField] private TMP_InputField mDescriptionField;
        [FormerlySerializedAs("changeTaskDropDown")] [SerializeField] private TMP_Dropdown mChangeTaskDropDown;
        [FormerlySerializedAs("addRewardButton")] [SerializeField] private Button mAddRewardButton;
        [SerializeField] private Button mEditImageButton;
        [SerializeField] private QuestBookRewardUI mQuestBookRewardUI;
        private QuestBookNodeContent Content {get => node.Content; set => node.Content = value;}
        private QuestBookPageUI questBookPageUI;
        private QuestBookNode node;
        public QuestBookPageUI QuestBookPageUI { get => questBookPageUI; set => questBookPageUI = value; }
        
        public void Initialize(QuestBookNode node, QuestBookPageUI questBookPageUI) {
            this.node = node;
            this.mTitleField.text = Content.Title;
            this.mDescriptionField.text = Content.Description;
            this.questBookPageUI = questBookPageUI;
            this.node = node;
            

            AssetManager.load();

            if (Content.Task == null) {
                this.mTitleField.text = "No Task";
            } else {
                SetTaskContent();
            }
            
            mQuestBookRewardUI.Initialize(node.Content,this);
            mTitleField.interactable = QuestBookHelper.EditMode;
            mDescriptionField.interactable = QuestBookHelper.EditMode;
            DisplayRewards();
            if (QuestBookHelper.EditMode) {
                mTitleField.onValueChanged.AddListener((string value) => {Content.Title = value;});
                mDescriptionField.onValueChanged.AddListener((string value) => {Content.Description = value;});

                mChangeTaskDropDown.ClearOptions();
                List<TMPro.TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                foreach (QuestTaskType taskType in Enum.GetValues(typeof(QuestTaskType))) {
                    options.Add(new TMPro.TMP_Dropdown.OptionData(taskType.ToString()));
                }
                mChangeTaskDropDown.AddOptions(options);

                QuestTaskType? currentTask = Content.Task == null ? null : Content.Task.getTaskType();
                int currentTaskIndex = currentTask == null ? 0 : (int) currentTask;
                mChangeTaskDropDown.value = currentTaskIndex;
                mChangeTaskDropDown.onValueChanged.AddListener(DropDownValueChanged);
            }
            
            mBackButton.onClick.AddListener(() => {
                questBookPageUI.gameObject.SetActive(true);
                GameObject.Destroy(gameObject);
            });
            
            if (!QuestBookHelper.EditMode)
            {
                mChangeTaskDropDown.interactable = false;
                mAddRewardButton.gameObject.SetActive(false);
                mEditImageButton.gameObject.SetActive(false);
            }
            else
            {
                mEditButton.onClick.AddListener(() => {
                    EditConnectionsPageUI connectionsPageUI = AssetManager.cloneElement<EditConnectionsPageUI>("NODE_EDITOR");
                    connectionsPageUI.init(node,questBookPageUI);
                    Canvas canvas = GameObject.FindAnyObjectByType<Canvas>();
                    connectionsPageUI.transform.SetParent(canvas.transform,false);
                }); 
                mEditImageButton.onClick.AddListener(() => {
                    SerializedItemSlotEditorUI serializedItemSlotEditor = AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    if (node.ImageSeralizedItemSlot == null)
                    {
                        node.ImageSeralizedItemSlot = new SerializedItemSlot("stone",1,null);
                    }

                    void Callback(SerializedItemSlot itemSlot) // Experimenting with inline function definitions
                    {
                        node.ImageSeralizedItemSlot = itemSlot;
                        questBookPageUI.RefreshNode(node);
                    }

                    serializedItemSlotEditor.Init(new List<SerializedItemSlot>{node.ImageSeralizedItemSlot},0,null,
                        gameObject,displayAmount:false,displayTags:false,displayArrows:false, displayTrash:false, callback: Callback);
                    serializedItemSlotEditor.transform.SetParent(transform,false);
                }); 
            }
            
        }
        
        
        private void SetTaskContent() {
            for (int i = 0; i < mTaskContainer.childCount; i++) {
                GameObject.Destroy(mTaskContainer.GetChild(i).gameObject);
            }
            this.mTitleField.text = Content.Task.getTaskType().ToString().Replace("_"," ");
            GameObject questContent = QuestBookTaskUIFactory.getContent(Content.Task, this);
            questContent.transform.SetParent(mTaskContainer,false);
        }

        private void DropDownValueChanged(int value) {
            QuestTaskType selectedTaskType = (QuestTaskType)Enum.Parse(typeof(QuestTaskType), mChangeTaskDropDown.options[value].text);
            if (Content.Task != null && Content.Task.getTaskType() == selectedTaskType) {
                return;
            }
            switch (selectedTaskType) {
                case QuestTaskType.Checkmark:
                    Content.Task = new CheckMarkQuestTask();
                    break;
                case QuestTaskType.Item:
                    Content.Task = new ItemQuestTask();
                    break;
                case QuestTaskType.Dimension:
                    Content.Task = new VisitDimensionQuestTask();
                    break;
            }
            SetTaskContent();
        }

        
        public void DisplayRewards() {
            /*
            GlobalHelper.deleteAllChildren(mRewardsList.transform);
            Content.Rewards ??= new List<SerializedItemSlot>();
            for (int i = 0; i < Content.Rewards.Count; i++) {
                RewardListElement rewardListElement = GameObject.Instantiate(rewardListElementPrefab, mRewardsList.transform, false);
                rewardListElement.Initialize(Content.Rewards,i,this);
            }
            */
        }
    }
}


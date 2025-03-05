using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using DevTools;
using Newtonsoft.Json;
using UI.QuestBook.Tasks;
using UI.QuestBook.Tasks.Rewards;
using UnityEngine.Serialization;


namespace UI.QuestBook {
    public class QuestBookTaskPageUI : MonoBehaviour
    {
        public UIAssetManager AssetManager;
        [SerializeField] private VerticalLayoutGroup mRewardsList;
        [FormerlySerializedAs("backButton")] [SerializeField] private Button mBackButton;
        [FormerlySerializedAs("taskContainer")] [SerializeField] private Transform mTaskContainer;
        [FormerlySerializedAs("editButton")] [SerializeField] private Button mEditButton;
        [FormerlySerializedAs("titleField")] [SerializeField] private TMP_InputField mTitleField;
        [FormerlySerializedAs("descriptionField")] [SerializeField] private TMP_InputField mDescriptionField;
        [FormerlySerializedAs("changeTaskDropDown")] [SerializeField] private TMP_Dropdown mChangeTaskDropDown;
        [SerializeField] private Button mCheckSubmissionButton;
        [SerializeField] private Button mEditImageButton;
        [SerializeField] private Button mEditSizeButton;
        [SerializeField] private QuestBookRewardUI mQuestBookRewardUI;
        [SerializeField] private Image mSelectTaskDownArrowImage;
        public QuestBookNodeContent Content
        {
            get => node.Content;
        }
        private QuestBookPageUI questBookPageUI;
        private QuestBookNode node;
        public QuestBookPageUI QuestBookPageUI { get => questBookPageUI; set => questBookPageUI = value; }
        
        public void Initialize(QuestBookNode node, QuestBookPageUI questBookPageUI, string questBookPath) {
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

            bool editMode = DevToolUtils.OnDevToolScene;
            mQuestBookRewardUI.Initialize(node,this);
            
            mTitleField.interactable = editMode;
            mDescriptionField.interactable = editMode;
            
            mChangeTaskDropDown.value = (int) node.Content.Task.GetTaskType();
            
            mTitleField.onValueChanged.AddListener((string value) => {Content.Title = value;});
            mDescriptionField.onValueChanged.AddListener((string value) => {Content.Description = value;});

            mChangeTaskDropDown.ClearOptions();
            List<TMPro.TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            foreach (QuestTaskType taskType in Enum.GetValues(typeof(QuestTaskType))) {
                options.Add(new TMPro.TMP_Dropdown.OptionData($"{taskType} Task"));
            }
            mChangeTaskDropDown.AddOptions(options);

            QuestTaskType? currentTask = Content.Task?.GetTaskType();
            int currentTaskIndex = currentTask == null ? 0 : (int) currentTask;
            mChangeTaskDropDown.value = currentTaskIndex;
            mChangeTaskDropDown.onValueChanged.AddListener(DropDownValueChanged);
            
            
            mBackButton.onClick.AddListener(() => {
                questBookPageUI.gameObject.SetActive(true);
                GameObject.Destroy(gameObject);
            });
            
            if (!editMode)
            {
                mChangeTaskDropDown.interactable = false;
                mEditImageButton.gameObject.SetActive(false);
                mEditButton.gameObject.SetActive(false);
                mEditSizeButton.gameObject.SetActive(false);
                mSelectTaskDownArrowImage.gameObject.SetActive(false);
            }
            else
            {
                mChangeTaskDropDown.interactable = true;
                mEditButton.onClick.AddListener(() => {
                    EditConnectionsPageUI connectionsPageUI = AssetManager.cloneElement<EditConnectionsPageUI>("NODE_EDITOR");
                    connectionsPageUI.Initialize(node,questBookPageUI,questBookPath);
                    Canvas canvas = GameObject.FindAnyObjectByType<Canvas>();
                    connectionsPageUI.transform.SetParent(canvas.transform,false);
                }); 
                mEditImageButton.onClick.AddListener(() => {
                    SerializedItemSlotEditorUI serializedItemSlotEditor = AssetManager.cloneElement<SerializedItemSlotEditorUI>("ITEM_EDITOR");
                    if (node.NodeData.ImageSeralizedItemSlot == null)
                    {
                        node.NodeData.ImageSeralizedItemSlot = new SerializedItemSlot("stone",1,null);
                    }

                    void Callback(SerializedItemSlot itemSlot) // Experimenting with inline function definitions
                    {
                        node.NodeData.ImageSeralizedItemSlot = itemSlot;
                        questBookPageUI.Display();
                    }

                    serializedItemSlotEditor.Init(new List<SerializedItemSlot>{node.NodeData.ImageSeralizedItemSlot},0,null,
                        gameObject,displayAmount:false,displayTags:false,displayArrows:false, displayTrash:false, callback: Callback);
                    serializedItemSlotEditor.transform.SetParent(transform,false);
                }); 
                
                mEditSizeButton.onClick.AddListener(() =>
                {
                    QuestBookEditSizeUI editSizeUI = AssetManager.cloneElement<QuestBookEditSizeUI>("SIZE_EDITOR");
                    editSizeUI.Initialize(node,this);
                    Canvas canvas = GameObject.FindAnyObjectByType<Canvas>();
                    editSizeUI.transform.SetParent(canvas.transform,false);
                });
            }

            UpdateSubmissionButton();
            mCheckSubmissionButton.onClick.AddListener(CheckTask);
            
        }

        public void OnTaskStatusChanged()
        {
            SetTaskContent();
            UpdateSubmissionButton();
            questBookPageUI.DisplayLines();
            mQuestBookRewardUI.UpdateClaimButtonImage();
        }
        private void CheckTask()
        {
            if (node.TaskData == null) return;
            if (!QuestBookUtils.EditMode)
            {
                if (node.TaskData is ICompletionCheckQuest completionCheckQuest)
                {
                    bool updated = completionCheckQuest.CheckCompletion();
                    if (updated)
                    {
                        OnTaskStatusChanged();
                    }
                }
            }
            else
            {
                node.TaskData.Complete = !node.TaskData.Complete;
            }
            

            UpdateSubmissionButton();
        }

        private void UpdateSubmissionButton()
        {
            
            if (node.TaskData is not ICompletionCheckQuest)
            {
                mCheckSubmissionButton.gameObject.SetActive(false);
                return;
            }

            var complete = node.TaskData.Complete;
            mCheckSubmissionButton.interactable = !complete || QuestBookUtils.EditMode;
            mCheckSubmissionButton.GetComponentInChildren<TextMeshProUGUI>().text =
                complete ? "Quest Completed" : "Check Submission";
            mCheckSubmissionButton.GetComponent<Image>().color = complete ? Color.green : Color.blue;
        }
        
        
        private void SetTaskContent() {
            for (int i = 0; i < mTaskContainer.childCount; i++) {
                GameObject.Destroy(mTaskContainer.GetChild(i).gameObject);
            }
            GameObject questContent = QuestBookTaskUIFactory.GetContent(Content.Task, node.TaskData, this);
            questContent.transform.SetParent(mTaskContainer,false);
        }

        private void DropDownValueChanged(int value)
        {
            QuestTaskType selectedTaskType = (QuestTaskType)value;
            if (Content.Task != null && Content.Task.GetTaskType() == selectedTaskType) {
                return;
            }
            switch (selectedTaskType) {
                case QuestTaskType.Checkmark:
                    Content.Task = new CheckMarkQuestTask();
                    break;
                case QuestTaskType.Item:
                    Content.Task = new ItemQuestTask(new List<SerializedItemSlot>());
                    break;
                case QuestTaskType.Dimension:
                    Content.Task = new VisitDimensionQuestTask();
                    break;
            }
            SetTaskContent();
        }
    }
}


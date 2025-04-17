using System;
using System.Collections;
using System.Collections.Generic;
using DevTools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data.Node;
using UnityEngine.Serialization;

namespace UI.QuestBook {
    public class ItemQuestTaskUI : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup mItemList;
        [FormerlySerializedAs("addButton")] [SerializeField] private Button mAddButton;
        [SerializeField] private ItemQuestItemElement itemQuestItemPrefab;
        public QuestBookTaskPageUI QuestBookTaskPageUI;
        private ItemQuestTask task;
        private QuestBookTaskData taskData;
        private List<ItemQuestItemElement> itemUIElements = new List<ItemQuestItemElement>();
        public void Display() {
            for (int i = 0; i < mItemList.transform.childCount; i++) {
                GameObject.Destroy(mItemList.transform.GetChild(i).gameObject);
            }
            itemUIElements.Clear();
            for (int i = 0; i < task?.Items?.Count; i++) {
                ItemQuestItemElement itemQuestItemElement = GameObject.Instantiate(itemQuestItemPrefab, mItemList.transform, false);
                itemQuestItemElement.Initialize(
                    task,
                    i,
                    this,
                    taskData
                );
                itemUIElements.Add(itemQuestItemElement);
            }
        }

        private void AddItem() {
            task.Items.Add(new SerializedItemSlot("stone",1,null));
            Display();
        }

        public void Display(QuestBookTaskPageUI questBookUI, ItemQuestTask contentData, QuestBookTaskData taskData)
        {
            this.QuestBookTaskPageUI = questBookUI;
            this.task = contentData;
            this.taskData = taskData;
            
            mAddButton.onClick.AddListener(AddItem);
            mAddButton.gameObject.SetActive(DevToolUtils.OnDevToolScene);
            Display();
        }

        public void FixedUpdate()
        {
            if (taskData == null || taskData.Complete) return;
            bool allComplete = true;
            foreach (ItemQuestItemElement itemUIElement in itemUIElements)
            {
                itemUIElement.Reload();
                if (!itemUIElement.Complete)
                {
                    allComplete = false;
                }
            }

            if (!allComplete) return;
            taskData.Complete = true;
            QuestBookTaskPageUI.RefreshRewardUI();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        private ItemQuestTaskData itemQuestTaskData;
        public void Display() {
            for (int i = 0; i < mItemList.transform.childCount; i++) {
                GameObject.Destroy(mItemList.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < task.Items.Count; i++) {
                ItemQuestItemElement itemQuestItemElement = GameObject.Instantiate(itemQuestItemPrefab, mItemList.transform, false);
                itemQuestItemElement.Initialize(
                    task,
                    i,
                    this,
                    QuestBookTaskPageUI.QuestBookPageUI
                );
            }
        }

        private void AddItem() {
            task.Items.Add(new SerializedItemSlot("stone",1,null));
            Display();
        }

        public void Display(QuestBookTaskPageUI questBookUI, ItemQuestTask contentData, QuestBookTaskData taskData, ItemQuestTaskData itemQuestTaskData)
        {
            this.QuestBookTaskPageUI = questBookUI;
            this.task = contentData;
            this.itemQuestTaskData = itemQuestTaskData;
            this.taskData = taskData;
            
            mAddButton.onClick.AddListener(AddItem);
            mAddButton.gameObject.SetActive(QuestBookUtils.EditMode);
            Display();
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class ItemQuestTaskUI : QuestBookTaskUI<ItemQuestTask>
    {
        [SerializeField] private GridLayoutGroup itemContainer;
        [SerializeField] private Button addButton;
        [SerializeField] private ItemQuestItemElement itemQuestItemPrefab;
        public QuestBookTaskPageUI QuestBookTaskPageUI;
        private ItemQuestTask task;
        public override void init(ItemQuestTask task, QuestBookTaskPageUI questBookTaskPageUI)
        {
            this.QuestBookTaskPageUI = questBookTaskPageUI;
            this.task = task;
            
            addButton.onClick.AddListener(addItem);
            addButton.gameObject.SetActive(QuestBookHelper.EditMode);
            display();
        }

        public void display() {
            for (int i = 0; i < itemContainer.transform.childCount; i++) {
                GameObject.Destroy(itemContainer.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < task.Items.Count; i++) {
                ItemQuestItemElement itemQuestItemElement = GameObject.Instantiate(itemQuestItemPrefab);
                itemQuestItemElement.init(
                    task,
                    i,
                    this,
                    QuestBookTaskPageUI.QuestBookPageUI
                );
                itemQuestItemElement.transform.SetParent(itemContainer.transform,false);
            }
        }

        private void addItem() {
            task.Items.Add(new SerializedItemSlot("stone",1,null));
            task.GottenAmounts.Add(0);
            display();
        }
    }
}


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
        private QuestBookPageUI questBookUI;
        private ItemQuestTask task;
        public override void init(ItemQuestTask task, QuestBookPageUI questBookUI)
        {
            this.questBookUI = questBookUI;
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
                GameObject element = GlobalHelper.instantiateFromResourcePath("UI/Quest/Tasks/Item/ItemElement");
                ItemQuestItemElement itemQuestItemElement = element.GetComponent<ItemQuestItemElement>();
                itemQuestItemElement.init(
                    task,
                    i,
                    this,
                    questBookUI
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


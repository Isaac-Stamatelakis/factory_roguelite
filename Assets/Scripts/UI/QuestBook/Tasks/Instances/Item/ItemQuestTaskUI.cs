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
        private QuestBookUI questBookUI;
        private ItemQuestTask task;
        public override void init(ItemQuestTask task, QuestBookUI questBookUI)
        {
            this.questBookUI = questBookUI;
            this.task = task;
            
            addButton.onClick.AddListener(addItem);
            if (!questBookUI.EditMode) {
                addButton.gameObject.SetActive(false);
            }
            display();
        }

        private void display() {
            for (int i = 0; i < itemContainer.transform.childCount; i++) {
                GameObject.Destroy(itemContainer.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < task.Items.Count; i++) {
                GameObject element = GlobalHelper.loadFromResourcePath("UI/Quest/Tasks/Item/ItemElement");
                ItemQuestItemElement itemQuestItemElement = element.GetComponent<ItemQuestItemElement>();
                itemQuestItemElement.init(
                    task,
                    i,
                    questBookUI
                );
                itemQuestItemElement.transform.SetParent(itemContainer.transform,false);
            }
        }

        public void OnDestroy() {
            foreach (SerializedItemSlot serializedItemSlot in task.Items) {
                Debug.Log(serializedItemSlot.id);
            }
        }

        private void addItem() {
            task.Items.Add(new SerializedItemSlot("stone",1,null));
            task.GottenAmounts.Add(0);
            display();
        }
    }
}


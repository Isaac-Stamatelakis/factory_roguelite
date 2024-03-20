using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTaskUI<Task> : MonoBehaviour where Task : QuestBookTask
    {
        public abstract void init(Task task);
    }

    public static class QuestBookTaskUIFactory {
        public static GameObject getContent(QuestBookTask task) {
            QuestTaskType taskType = task.getTaskType();
            switch (taskType) {
                case QuestTaskType.Item:
                    GameObject itemObject = GameObject.Instantiate(Resources.Load<GameObject>("UI/Quest/Tasks/Item/ItemQuestTask"));
                    ItemQuestTaskUI itemQuestTaskUI = itemObject.GetComponent<ItemQuestTaskUI>();
                    itemQuestTaskUI.init((ItemQuestTask)task);
                    return itemObject;
                case QuestTaskType.Checkmark:
                    GameObject checkObject = GameObject.Instantiate(Resources.Load<GameObject>("UI/Quest/Tasks/CheckMarkTaskUI"));
                    CheckMarkTaskUI checkMarkTaskUI = checkObject.GetComponent<CheckMarkTaskUI>();
                    checkMarkTaskUI.init((CheckMarkQuestTask)task);
                    return checkObject;
                case QuestTaskType.Dimension:
                    GameObject dimObject = GameObject.Instantiate(Resources.Load<GameObject>("UI/Quest/Tasks/VisitDimensionTask"));
                    VisitDimensionTaskUI visitDimensionTaskUI = dimObject.GetComponent<VisitDimensionTaskUI>();
                    visitDimensionTaskUI.init((VisitDimensionQuestTask)task);
                    return dimObject;
            }
            return null;
        }
    }
}


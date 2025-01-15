using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTaskUI<Task> : MonoBehaviour where Task : QuestBookTask
    {
        public abstract void init(Task task, QuestBookTaskPageUI taskPageUI);
    }

    public static class QuestBookTaskUIFactory {
        public static GameObject getContent(QuestBookTask task, QuestBookTaskPageUI taskPageUI) {
            QuestTaskType taskType = task.getTaskType();
            UIAssetManager assetManager = taskPageUI.QuestBookPageUI.QuestBookUI.AssetManager;
            switch (taskType) {
                case QuestTaskType.Item:
                    ItemQuestTaskUI itemQuestTaskUI = assetManager.cloneElement<ItemQuestTaskUI>("ITEM_TASK");
                    itemQuestTaskUI.init((ItemQuestTask)task,taskPageUI);
                    return itemQuestTaskUI.gameObject;
                case QuestTaskType.Checkmark:
                    CheckMarkTaskUI checkMarkTaskUI = assetManager.cloneElement<CheckMarkTaskUI>("CHECKMARK_TASK");
                    checkMarkTaskUI.init((CheckMarkQuestTask)task,taskPageUI);
                    return checkMarkTaskUI.gameObject;
                case QuestTaskType.Dimension:
                    VisitDimensionTaskUI visitDimensionTaskUI = assetManager.cloneElement<VisitDimensionTaskUI>("DIM_TASK");
                    visitDimensionTaskUI.init((VisitDimensionQuestTask)task,taskPageUI);
                    return visitDimensionTaskUI.gameObject;
            }
            return null;
        }
    }
}


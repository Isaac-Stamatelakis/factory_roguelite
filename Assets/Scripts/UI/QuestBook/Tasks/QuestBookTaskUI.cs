using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public abstract class QuestBookTaskUI<TTaskContentData, TTaskData> : MonoBehaviour where TTaskContentData : QuestBookTask where TTaskData : QuestBookTaskData
    {
        public abstract void Display(QuestBookTaskPageUI questBookUI, TTaskContentData contentData, TTaskData taskData);
    }

    public static class QuestBookTaskUIFactory {
        public static GameObject getContent(QuestBookTask task, QuestBookTaskData taskData, QuestBookTaskPageUI taskPageUI) {
            QuestTaskType taskType = task.GetTaskType();
            UIAssetManager assetManager = taskPageUI.QuestBookPageUI.QuestBookUI.AssetManager;
            switch (taskType) {
                case QuestTaskType.Item:
                    ItemQuestTaskUI itemQuestTaskUI = assetManager.cloneElement<ItemQuestTaskUI>("ITEM_TASK");
                    itemQuestTaskUI.Display(taskPageUI,(ItemQuestTask)task,(ItemQuestTaskData)taskData);
                    return itemQuestTaskUI.gameObject;
                case QuestTaskType.Checkmark:
                    CheckMarkTaskUI checkMarkTaskUI = assetManager.cloneElement<CheckMarkTaskUI>("CHECKMARK_TASK");
                    checkMarkTaskUI.Display(taskPageUI,taskData);
                    return checkMarkTaskUI.gameObject;
                case QuestTaskType.Dimension:
                    VisitDimensionTaskUI visitDimensionTaskUI = assetManager.cloneElement<VisitDimensionTaskUI>("DIM_TASK");
                    visitDimensionTaskUI.Display(taskPageUI,(VisitDimensionQuestTask)task,taskData);
                    return visitDimensionTaskUI.gameObject;
            }
            return null;
        }
    }
}


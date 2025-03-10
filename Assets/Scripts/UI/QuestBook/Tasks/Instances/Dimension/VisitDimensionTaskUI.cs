using System;
using System.Collections;
using System.Collections.Generic;
using DevTools;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI.QuestBook.Data.Node;
using WorldModule.Caves;

namespace UI.QuestBook {
    public class VisitDimensionTaskUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI dimensionTitle;
        [SerializeField] private TextMeshProUGUI statusText;
        public void Display(QuestBookTaskPageUI questBookTaskPageUI, VisitDimensionQuestTask task, QuestBookTaskData taskData)
        {
            if (!taskData.Complete && !DevToolUtils.OnDevToolScene)
            {
                bool SuccessCallback(string cacheData) => cacheData.Contains(task.CaveId);
                QuestBookCache cache = PlayerManager.Instance.GetPlayer().QuestBookCache;
                taskData.Complete = cache.QueueSatisfiedCache(QuestTaskType.Dimension,SuccessCallback);
                Debug.Log(taskData.Complete);
            }
            if (taskData.Complete) {
                statusText.text = "Visited";
                statusText.color = Color.green;
            } else {
                statusText.text = "Not Visited";
                statusText.color = Color.red;
            }
        }
        
        
    }
}


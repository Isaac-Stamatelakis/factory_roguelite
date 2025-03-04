using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WorldModule.Caves;

namespace UI.QuestBook {
    public class VisitDimensionTaskUI : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI dimensionTitle;
        [SerializeField] private TextMeshProUGUI statusText;
        public void Display(QuestBookTaskPageUI questBookTaskPageUI, VisitDimensionQuestTask task, QuestBookTaskData taskData)
        {
            if (taskData.Complete) {
                statusText.text = "Visited";
                statusText.color = Color.green;
            } else {
                statusText.text = "Not Visited";
                statusText.color = Color.red;
            }
            string caveId = task.CaveId;
        }
    }
}


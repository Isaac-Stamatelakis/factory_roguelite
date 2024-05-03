using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.QuestBook {
    public class VisitDimensionTaskUI : QuestBookTaskUI<VisitDimensionQuestTask>
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI dimensionTitle;
        [SerializeField] private TextMeshProUGUI statusText;
        public override void init(VisitDimensionQuestTask task, QuestBookPageUI questBookUI)
        {
            if (task.Visited) {
                statusText.text = "Visited";
                statusText.color = Color.green;
            } else {
                statusText.text = "Not Visited";
                statusText.color = Color.red;
            }
            dimensionTitle.text = task.Cave.ToString();
        }
    }
}


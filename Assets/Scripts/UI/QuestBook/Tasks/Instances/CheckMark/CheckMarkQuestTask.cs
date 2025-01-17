using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class CheckMarkQuestTask : QuestBookTask
    {
        private bool clicked;
        public bool Clicked { get => clicked; set => clicked = value; }

        public override bool IsComplete()
        {
            return clicked;
        }

        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Checkmark;
        }

        public override void SetCompletion(bool state)
        {
            clicked = state;
        }
    }

}

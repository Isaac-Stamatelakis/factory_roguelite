using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class CheckMarkQuestTask : QuestBookTask
    {
        private bool clicked;
        public bool Clicked { get => clicked; set => clicked = value; }

        public override bool getComplete()
        {
            return clicked;
        }

        public override QuestTaskType getTaskType()
        {
            return QuestTaskType.Checkmark;
        }

        public override void setComplete()
        {
            clicked = true;
        }

        public override void setUnComplete()
        {
            clicked = false;
        }
    }

}

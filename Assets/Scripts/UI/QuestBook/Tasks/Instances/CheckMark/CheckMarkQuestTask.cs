using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class CheckMarkQuestTask : QuestBookTask
    {
        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Checkmark;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances;
using WorldModule.Caves;

namespace UI.QuestBook {
    public class VisitDimensionQuestTask : QuestBookTask
    {
        public string CaveId;
        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Dimension;
        }

        public bool CheckCompletion()
        {
            return false;
        }
    }
}


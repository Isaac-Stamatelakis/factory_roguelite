using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntity.Instances;
using WorldModule.Caves;

namespace UI.QuestBook {
    public class VisitDimensionQuestTask : QuestBookTask, ICompletionCheckQuest
    {
        private bool visited;
        private string caveId;
        public bool Visited { get => visited; set => visited = value; }
        public string CaveId { get => caveId; set => caveId = value; }

        public override bool IsComplete()
        {
            return visited;
        }

        public override QuestTaskType GetTaskType()
        {
            return QuestTaskType.Dimension;
        }

        public override void SetCompletion(bool state)
        {
            visited = state;
        }

        public bool CheckCompletion()
        {
            return false;
        }
    }
}


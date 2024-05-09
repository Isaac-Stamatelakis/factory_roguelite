using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances;
using WorldModule.Caves;

namespace UI.QuestBook {
    public class VisitDimensionQuestTask : QuestBookTask
    {
        private bool visited;
        private string caveId;
        public bool Visited { get => visited; set => visited = value; }
        public string CaveId { get => caveId; set => caveId = value; }

        public override bool getComplete()
        {
            return visited;
        }

        public override QuestTaskType getTaskType()
        {
            return QuestTaskType.Dimension;
        }

        public override void setComplete()
        {
            visited = true;
        }

        public override void setUnComplete()
        {
            visited = false;
        }
    }
}


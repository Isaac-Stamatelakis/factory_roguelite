using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileEntityModule.Instances;

namespace UI.QuestBook {
    public class VisitDimensionQuestTask : QuestBookTask
    {
        private bool visited;
        private CaveRegion cave;
        public bool Visited { get => visited; set => visited = value; }
        public CaveRegion Cave { get => cave; set => cave = value; }

        public override bool getComplete()
        {
            return visited;
        }

        public override QuestTaskType getTaskType()
        {
            return QuestTaskType.Dimension;
        }
    }
}


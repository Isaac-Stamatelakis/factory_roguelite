using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    public class QuestBookNode
    {
        private HashSet<int> prerequisites;
        private string itemImageID;
        private float x;
        private float y;
        private int id;
        private QuestBookNodeContent content;
        private bool requireAllPrerequisites;
        public HashSet<int> Prerequisites { get => prerequisites; set => prerequisites = value; }
        public string ItemImageID { get => itemImageID; set => itemImageID = value; }
        public QuestBookNodeContent Content { get => content; set => content = value; }
        public Vector2 Position {get => new Vector2(x,y);}
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public int Id { get => id; set => id = value; }
        public bool RequireAllPrerequisites { get => requireAllPrerequisites; set => requireAllPrerequisites = value; }

        public QuestBookNode(Vector2 position, string itemImageID, QuestBookNodeContent content, HashSet<int> prerequisites, int id, bool requireAllPrerequisites) {
            this.X = position.x;
            this.Y = position.y;
            this.itemImageID = itemImageID;
            this.prerequisites = prerequisites;
            this.content = content;
            this.id = id;
            this.requireAllPrerequisites = requireAllPrerequisites;
        }
    }
}


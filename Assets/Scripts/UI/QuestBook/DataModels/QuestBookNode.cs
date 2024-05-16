using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UI.NodeNetwork;

namespace UI.QuestBook {
    public class QuestBookNode : INode
    {
        private HashSet<int> prerequisites;
        private string imageSeralizedItemSlot;
        private float x;
        private float y;
        private int id;
        private QuestBookNodeContent content;
        private bool requireAllPrerequisites;
        public HashSet<int> Prerequisites { get => prerequisites; set => prerequisites = value; }
        public string ImageSeralizedItemSlot { get => imageSeralizedItemSlot; set => imageSeralizedItemSlot = value; }
        public QuestBookNodeContent Content { get => content; set => content = value; }
        public Vector2 Position {get => new Vector2(x,y);}
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }
        public int Id { get => id; set => id = value; }
        public bool RequireAllPrerequisites { get => requireAllPrerequisites; set => requireAllPrerequisites = value; }

        public QuestBookNode(Vector2 position, string imageSeralizedItemSlot, QuestBookNodeContent content, HashSet<int> prerequisites, int id, bool requireAllPrerequisites) {
            this.X = position.x;
            this.Y = position.y;
            this.imageSeralizedItemSlot = imageSeralizedItemSlot;
            this.prerequisites = prerequisites;
            this.content = content;
            this.id = id;
            this.requireAllPrerequisites = requireAllPrerequisites;
        }

        public Vector3 getPosition()
        {
            return new Vector3(x,y,0);
        }

        public int getId()
        {
            return id;
        }

        public HashSet<int> getPrerequisites()
        {
            return prerequisites;
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    public class QuestBookNode
    {
        private List<string> connections;
        private string itemImageID;
        private float x;
        private float y;
        private QuestBookNodeContent content;
        public List<string> Connections { get => connections; set => connections = value; }
        public string ItemImageID { get => itemImageID; set => itemImageID = value; }
        public QuestBookNodeContent Content { get => content; set => content = value; }
        public float X { get => x; set => x = value; }
        public float Y { get => y; set => y = value; }

        public QuestBookNode(Vector2 position, string itemImageID, QuestBookNodeContent content, List<string> connections) {
            this.X = position.x;
            this.Y = position.y;
            this.itemImageID = itemImageID;
            this.connections = connections;
            this.content = content;
        }
    }
}


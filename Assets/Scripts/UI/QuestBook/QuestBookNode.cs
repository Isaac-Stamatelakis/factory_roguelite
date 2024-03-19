using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    public class QuestBookNode
    {
        private List<string> connections;
        private string itemImageID;
        private int x;
        private int y;
        private QuestBookNodeContent content;
        public List<string> Connections { get => connections; set => connections = value; }
        public string ItemImageID { get => itemImageID; set => itemImageID = value; }
        public QuestBookNodeContent Content { get => content; set => content = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
    }
}


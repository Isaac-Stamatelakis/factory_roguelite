using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UI.NodeNetwork;

namespace UI.QuestBook {
    public class QuestBookPage : INodeNetwork<QuestBookNode>
    {
        public string Title;
        public List<QuestBookNode> Nodes;

        public QuestBookPage(string title, List<QuestBookNode> nodes) {
            this.Title = title;
            this.Nodes = nodes;
        }

        public List<QuestBookNode> getNodes()
        {
            return Nodes;
        }
    }

}

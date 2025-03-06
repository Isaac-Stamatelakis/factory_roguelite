using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UI.NodeNetwork;
using UI.QuestBook.Data.Node;

namespace UI.QuestBook {
    public class QuestBookPage : INodeNetwork<QuestBookNode>
    {
        public List<QuestBookNode> Nodes;

        public QuestBookPage(List<QuestBookNode> nodes) {
            this.Nodes = nodes;
        }

        public List<QuestBookNode> GetNodes()
        {
            return Nodes;
        }
    }

}

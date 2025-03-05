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
        public Dictionary<int, QuestBookNode> IdNodeDict;

        public QuestBookPage(List<QuestBookNode> nodes, Dictionary<int, QuestBookNode> idNodeDict) {
            this.Nodes = nodes;
            this.IdNodeDict = idNodeDict;
        }

        public List<QuestBookNode> GetNodes()
        {
            return Nodes;
        }
    }

}

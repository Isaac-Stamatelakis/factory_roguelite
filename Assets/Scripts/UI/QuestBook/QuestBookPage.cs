using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    public class QuestBookPage
    {
        private List<QuestBookNode> nodes;

        public List<QuestBookNode> Nodes { get => nodes; set => nodes = value; }
    }

}

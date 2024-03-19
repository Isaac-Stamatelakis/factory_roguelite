using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace UI.QuestBook {
    public class QuestBookPage
    {
        private string title;
        private List<QuestBookNode> nodes;

        public List<QuestBookNode> Nodes { get => nodes; set => nodes = value; }
        public string Title { get => title; set => title = value; }

        public QuestBookPage(string title, List<QuestBookNode> nodes) {
            this.title = title;
            this.nodes = nodes;
        }
    }

}

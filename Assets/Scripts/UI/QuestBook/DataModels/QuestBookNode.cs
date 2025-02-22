using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using Newtonsoft.Json;
using UI.NodeNetwork;

namespace UI.QuestBook {
    public class QuestBookNode : INode
    {
        public List<int> Prerequisites;
        public SerializedItemSlot ImageSeralizedItemSlot;
        public float X;
        public float Y;
        public int Id;
        public QuestBookNodeContent Content;
        public bool RequireAllPrerequisites;

        public QuestBookNode(Vector2 position, SerializedItemSlot serializedItemSlot, QuestBookNodeContent content, List<int> prerequisites, int id, bool requireAllPrerequisites) {
            this.X = position.x;
            this.Y = position.y;
            this.ImageSeralizedItemSlot = serializedItemSlot;
            this.Prerequisites = prerequisites;
            this.Content = content;
            this.Id = id;
            this.RequireAllPrerequisites = requireAllPrerequisites;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(X,Y,0);
        }

        public void SetPosition(Vector3 pos)
        {
            X = pos.x;
            Y = pos.y;
        }

        public int GetId()
        {
            return Id;
        }

        public List<int> GetPrerequisites()
        {
            return Prerequisites;
        }
    }
}


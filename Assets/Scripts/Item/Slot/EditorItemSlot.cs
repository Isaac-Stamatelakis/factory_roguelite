using System.Collections;
using System.Collections.Generic;
using Items;
using Items.Tags;
using UnityEngine;

namespace Items
{
    [System.Serializable]
    public class EditorItemSlot
    {
        public string Id;
        public uint Amount;
        
        public EditorItemSlot(string id, uint amount)
        {
            Id = id;
            Amount = amount;
        }
    }
    [System.Serializable]
    public class EditorKvp<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
    }
}

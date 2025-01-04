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
        public ItemObject ItemObject;
        public uint Amount;
        public List<EditorKVP<ItemTag, EditorTagData>> Tags;
    }
    [System.Serializable]
    public class EditorKVP<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;
    }
    public abstract class EditorTagData
    {
        
    }

}

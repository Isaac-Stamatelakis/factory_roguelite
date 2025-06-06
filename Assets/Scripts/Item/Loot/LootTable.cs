using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    [CreateAssetMenu(fileName ="New Loot Table",menuName="Item/Loot Table")]    
    public class LootTable : ScriptableObject
    {
        [SerializeField] public List<LootResult> loot;   
        public int MinItems;
        public int MaxItems;
        [SerializeField] public bool repetitions;
    }

    [System.Serializable]
    public class LootResult {
        [SerializeField] public ItemObject item;
        public int MinAmount;
        public int MaxAmount;
        [SerializeField] public int frequency;
    }

    
}


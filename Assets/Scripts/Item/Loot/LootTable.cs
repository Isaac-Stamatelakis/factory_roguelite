using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    [CreateAssetMenu(fileName ="New Loot Table",menuName="Item/Loot Table")]    
    public class LootTable : ScriptableObject
    {
        [SerializeField] public List<LootResult> loot;   
        [SerializeField] public Vector2Int lootRange;
        [SerializeField] public bool repetitions;
    }

    [System.Serializable]
    public class LootResult {
        [SerializeField] public ItemObject item;
        [SerializeField] public Vector2Int amountRange;
        [SerializeField] public int frequency;
    }
}


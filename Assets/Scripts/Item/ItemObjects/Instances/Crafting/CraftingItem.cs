using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items {
    
    public interface IStateItem {
        public ItemState getItemState();
    }
    [CreateAssetMenu(fileName ="New Material Item",menuName="Item/Instances/Crafting/Standard")]
    public class CraftingItem : PresetItemObject
    {
        [SerializeField] private ItemState state;
        public ItemState getItemState()
        {
            return state;
        }
    }
}

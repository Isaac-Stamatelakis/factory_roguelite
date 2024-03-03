using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    
    public interface IStateItem {
        public ItemState getItemState();
    }
    [CreateAssetMenu(fileName ="New Material Item",menuName="Item Register/Crafting/Standard")]
    public class CraftingItem : ItemObject, IStateItem
    {
        public Sprite sprite;
        [SerializeField] private ItemState state;

        public override Sprite getSprite()
        {
            return this.sprite;
        }

        public ItemState getItemState()
        {
            return state;
        }
    }
}

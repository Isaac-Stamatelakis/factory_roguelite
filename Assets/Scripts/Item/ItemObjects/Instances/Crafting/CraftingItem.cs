using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemModule {
    
    public interface IStateItem {
        public ItemState getItemState();
    }
    [CreateAssetMenu(fileName ="New Material Item",menuName="Item/Instances/Crafting/Standard")]
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

    public class AnimatedCraftingItem : ItemObject, IStateItem, IMultipleSpriteObject
    {
        [SerializeField] private ItemState state;
        [SerializeField] private Sprite[] sprites;
        public ItemState getItemState()
        {
            return state;
        }

        public override Sprite getSprite()
        {
            throw new System.NotImplementedException();
        }

        public Sprite[] getSprites()
        {
            return sprites;
        }
    }
}

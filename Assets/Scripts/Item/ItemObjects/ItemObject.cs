using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Items.Tags;
using TileEntity;
using UnityEngine;

namespace Items {
    public enum ItemDisplayType {
        Single,
        Stack,
        Animated
    }
    public abstract class ItemObject : ScriptableObject {
        [Header("Unique identifier for this item")]
        public string id;

        public GameStageObject gameStage;
        
        [SerializeField] private List<ItemTag> ApplyableTags;

        public bool CanApplyTag(ItemTag tag)
        {
            return ApplyableTags.Contains(tag);
        }
        public abstract Sprite[] getSprites();
        public abstract Sprite getSprite();
        public abstract ItemDisplayType? getDisplayType();
    }
}
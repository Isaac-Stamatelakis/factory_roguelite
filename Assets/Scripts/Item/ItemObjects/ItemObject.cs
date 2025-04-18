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
        
        [SerializeField] public List<ItemTag> ApplyableTags;
        public SpriteOverlay[] SpriteOverlays;
        public bool CanApplyTag(ItemTag tag)
        {
            return ApplyableTags.Contains(tag);
        }
        public bool CanApplyTags(HashSet<ItemTag> tags)
        {
            foreach (ItemTag tag in tags)
            {
                if (CanApplyTag(tag)) return true;
            }

            return false;
        }
        public abstract Sprite[] getSprites();
        public abstract Sprite getSprite();
        public abstract ItemDisplayType? getDisplayType();
        public abstract GameStageObject GetGameStageObject();
    }

    [System.Serializable]
    public class SpriteOverlay
    {
        public Color Color = Color.white;
        public Sprite Sprite;
    }
}
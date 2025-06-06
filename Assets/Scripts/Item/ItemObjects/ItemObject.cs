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
        public abstract Sprite[] GetSprites();
        public abstract Sprite GetSprite();
        public abstract ItemDisplayType? getDisplayType();
        public abstract GameStageObject GetGameStageObject();

        public Tier GetTier()
        {
            GameStageObject gameStageObject = GetGameStageObject();
            if (!gameStageObject) return Tier.Untiered;
            if (gameStageObject is TieredGameStage tieredGameStage) return tieredGameStage.Tier;
            if (gameStageObject.GetGameStageName() == "DISABLED" || gameStageObject.GetGameStageName() == "HIDE") return Tier.Disabled;
            return Tier.Untiered;
        }
        /// <summary>
        /// Sets game stage object of item object
        /// </summary>
        /// <remarks>This is only abstract to prevent bugs. Some ItemObject implementations eg TransmutationItemObject do not technically require this method</remarks>
        /// <param name="gameStageObject"></param>
        public abstract void SetGameStageObject(GameStageObject gameStageObject);
    }

    [System.Serializable]
    public class SpriteOverlay
    {
        public Color Color = Color.white;
        public Material Material;
        public Sprite Sprite;
    }
}
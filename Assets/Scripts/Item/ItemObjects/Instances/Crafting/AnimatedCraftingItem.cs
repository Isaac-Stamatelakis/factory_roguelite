using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Slot;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items {
    [CreateAssetMenu(fileName ="New Animated Item",menuName="Item/Instances/Crafting/Animated")]
    public class AnimatedCraftingItem : ItemObject
    {
        public GameStageObject Stage;
        public Sprite[] Sprites;
        public override Sprite[] getSprites()
        {
            return Sprites;
        }

        public override Sprite getSprite()
        {
            return Sprites.Length > 0 ? Sprites[0] : null;
        }

        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Animated;
        }

        public override GameStageObject GetGameStageObject()
        {
            return Stage;
        }
    }
}

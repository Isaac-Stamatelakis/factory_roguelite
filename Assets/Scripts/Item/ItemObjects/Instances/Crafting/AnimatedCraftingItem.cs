using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.ItemObjects.Display;
using Item.Slot;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items {
    public interface IAnimatedOverlayItem
    {
        public SpriteCollection[] SpriteCollectionOverlays { get; }
    }
    public interface IColorableItem
    {
        public Color Color { get; }
    }
    [CreateAssetMenu(fileName ="New Animated Item",menuName="Item/Instances/Crafting/Animated")]
    public class AnimatedCraftingItem : ItemObject, IAnimatedOverlayItem, IColorableItem
    
    {
        public GameStageObject Stage;
        public SpriteCollection[] SpriteCollection;
        public ColorScriptableObject ColorScriptableObject;
        public Sprite Sprite;
        public override Sprite[] GetSprites()
        {
            return new Sprite[] { Sprite };
        }

        public override Sprite GetSprite()
        {
            return Sprite;
        }

        public override ItemDisplayType? getDisplayType()
        {
            return ItemDisplayType.Single;
        }

        public override GameStageObject GetGameStageObject()
        {
            return Stage;
        }

        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            Stage = gameStageObject;
        }

        public SpriteCollection[] SpriteCollectionOverlays => SpriteCollection;

        public Color Color => ColorScriptableObject?.Color ?? Color.white;
    }
}

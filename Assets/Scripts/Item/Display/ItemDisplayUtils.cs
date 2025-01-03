using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Items {
    public static class ItemDisplayUtils {
        private static readonly string slotName = "slot";
        private static readonly string itemImageName = "item";
        private static readonly string itemAmountName = "amount";
        private static readonly string itemTagNameFront = "tagFront";
        private static readonly string itemTagNameBehind = "tagBehind";
        private static readonly string stackSpriteImage = "stackSprite";
        public static string ItemImageName { get => itemImageName; }
        public static string ItemAmountName { get => itemAmountName; }
        public static string ItemTagNameFront { get => itemTagNameFront; }
        public static string ItemTagNameBehind { get => itemTagNameBehind; }
        private static readonly Color solidItemPanelColor = new Color(200f/255f,200f/255f,200f/255f,100f/255f);
        private static readonly Color fluidItemPanelColor = new Color(115f/255f,115f/255f,115f/255f,100/255f);

        public static string[] AmountSuffixes => suffixes;
        private static readonly int animationSpeed = 10;

        public static string SlotName => slotName;

        public static string StackSpriteImageName => stackSpriteImage;

        public static Color SolidItemPanelColor => solidItemPanelColor;

        public static Color FluidItemPanelColor => fluidItemPanelColor;

        public static int AnimationSpeed { get => animationSpeed; }

        private static readonly string[] suffixes = {"k","M","B","T"};

        public static string FormatAmountText(uint amount,bool oneInvisible = true)
        {
            if (amount == 0) return string.Empty;
            if (amount == 1 && oneInvisible) {
                return string.Empty;
            }
            if (amount < 10000) {
                return amount.ToString();
            }
            int i = 0;
            float fAmount = amount/1000f;
            while (i < suffixes.Length-1 && fAmount >= 1000) {
                fAmount /= 1000;
                i++;
            }
            return fAmount.ToString("0.#" + suffixes[i]);
        }
        public static Vector2 getItemScale(Sprite sprite) {
            Vector2 size = getItemSize(sprite);
            if (size == Vector2.zero) {
                return Vector2.one;
            }
            Vector2 vector = size.x >= size.y ? new Vector2(1,size.y/size.x) : new Vector2(size.x/size.y,1);
            if (size.x == 32) {
                vector.x = 0.5f;
            }
            if (size.y == 32) {
                vector.y = 0.5f;
            }
            return vector;
            
        }
        /// <summary>
        /// Calculates the scale for a sprite to fit within a given size while maintaining its aspect ratio.
        /// </summary>
        /// <param name="sprite">The sprite to be scaled.</param>
        /// <param name="size">The maximum size in tiles the sprite should fit within.</param>
        /// <returns>The scale factors for the sprite to fit within the specified size.</returns>
        public static Vector2 getConstrainedItemScale(Sprite sprite, Vector2 size) {
            Vector2 itemSize = new Vector2(sprite.rect.width, sprite.rect.height)/16f;
            float scaleX = size.x / itemSize.x;
            float scaleY = size.y / itemSize.y;
            return new Vector2(scaleX,scaleY);
        }

        public static Vector2 getItemSize(Sprite sprite) {
            if (sprite == null) {
                return Vector2.zero;
            }
            Vector2 adjustedSpriteSize = sprite.bounds.size/0.5f;
            if (adjustedSpriteSize.x == 1 && adjustedSpriteSize.y == 1) {
                return new Vector2(32,32);
            }
            if (adjustedSpriteSize.x == adjustedSpriteSize.y) {
                return new Vector2(64,64);
            }
            if (adjustedSpriteSize.x > adjustedSpriteSize.y) {
                return new Vector2(64,adjustedSpriteSize.y/adjustedSpriteSize.x*64);
            }
            if (adjustedSpriteSize.y > adjustedSpriteSize.x) {
                return new Vector2(adjustedSpriteSize.x/adjustedSpriteSize.y*64,64);
            }
            return Vector2.zero;
        }

        public static void SetImageItemSprite(Image image, Sprite sprite)
        {
            image.sprite = sprite;
            image.transform.localScale = getItemScale(sprite);
        }

        public static void DisplayItemSprite(Image image, ItemObject itemObject, int counter)
        {
            switch (itemObject.getDisplayType()) {
                case ItemDisplayType.Single:
                    SetImageItemSprite(image,itemObject.getSprite());
                    break;
                case ItemDisplayType.Stack:
                    Image[] images = image.GetComponentsInChildren<Image>();
                    Sprite[] stackSprites = itemObject.getSprites();
                    for (int i = 0; i < stackSprites.Length; i++) {
                        int imageIndex = stackSprites.Length-i; // Sprites are orded by index with larger showing lower
                        SetImageItemSprite(images[imageIndex],stackSprites[i]);
                    }
                    break;
                case ItemDisplayType.Animated:
                    Sprite[] animationSprites = itemObject.getSprites();
                    int adjustedCounter = Mathf.FloorToInt(counter/(float) ItemDisplayUtils.AnimationSpeed);
                    int animationIndex = adjustedCounter % animationSprites.Length;
                    SetImageItemSprite(image,animationSprites[animationIndex]);
                    break;
            }
        }
        
    }
}

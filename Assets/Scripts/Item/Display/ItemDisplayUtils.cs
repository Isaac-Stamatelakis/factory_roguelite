using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;

namespace Items {
    public static class ItemDisplayUtils
    {

        public const string TIER_REPLACE_VALUE = "{TIER}";
        
        private static readonly string[] suffixes = {"k","M","B","T"};

        public static string FormatAmountText(uint amount,bool oneInvisible = true, ItemState itemState = ItemState.Solid)
        {
            switch (itemState)
            {
                case ItemState.Solid:
                    return FormatSolidItemText(amount, oneInvisible);
                case ItemState.Fluid:
                    return FormatFluidItemText(amount);
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemState), itemState, null);
            }
        }

        private static string FormatSolidItemText(uint amount, bool oneInvisible)
        {
            switch (amount)
            {
                case <= 1 when oneInvisible:
                    return string.Empty;
                case < 1000:
                    return amount.ToString();
            }

            int i = 0;
            float fAmount = amount/1000f;
            while (i < suffixes.Length-1 && fAmount >= 1000) {
                fAmount /= 1000;
                i++;
            }
            return fAmount.ToString("0.0" + suffixes[i]);
        }

        private static string FormatFluidItemText(uint amount)
        {
            const string FLUID_SUFFIX = "L";
            if (amount <= 1000)
            {
                return $"{amount}m{FLUID_SUFFIX}";
            }
            int i = 0;
            float fAmount = amount/1000f;
            while (i < suffixes.Length-1 && fAmount >= 1000) {
                fAmount /= 1000;
                i++;
            }

            i--; // Go one suffix down for fluids
            string suffix = i < 0 ? string.Empty : suffixes[i];
            return fAmount.ToString("0.00" + suffix + FLUID_SUFFIX);
        }
        public static Vector3 GetItemScale(Sprite sprite) {
            Vector2 size = GetItemSize(sprite);
            if (size == Vector2.zero) {
                return Vector3.one;
            }
            Vector3 vector = size.x >= size.y ? new Vector3(1,size.y/size.x,1) : new Vector3(size.x/size.y,1,1);
            bool smallX = (int)size.x == 32;
            bool smallY = (int)size.y == 32;
            
            if (smallX) {
                vector.x = 0.5f;
            }
            if (smallY) {
                vector.y = 0.5f;
            }
            return vector;
            
        }
        /// <summary>
        /// Returns the a scale for spriterenderer such that the Sprite fits within a 16x16 pixel box
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static Vector2 GetSpriteRenderItemScale(Sprite sprite)
        {
            Vector2 itemSize = new Vector2(sprite.rect.width, sprite.rect.height)/16f;
            if (itemSize.x < itemSize.y)
            {
                return new Vector2(1/itemSize.y,1/itemSize.y);
            } else
            {
                return new Vector2(1/itemSize.x,1/itemSize.x);
            }
        }

        public static Vector2 GetItemSize(Sprite sprite) {
            if (!sprite) {
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
            image.transform.localScale = GetItemScale(sprite);
        }
        
        
    }
}

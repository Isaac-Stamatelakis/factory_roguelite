using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;

namespace Items {
    public static class ItemDisplayUtils
    {
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
            if (amount < 1000)
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
            return fAmount.ToString("0.0" + suffix + FLUID_SUFFIX);
        }
        public static Vector3 GetItemScale(Sprite sprite) {
            Vector2 pixelSize = GetItemSize(sprite);
            if (pixelSize == Vector2.zero) {
                return Vector3.one;
            }
            Vector3 vector;
            if (pixelSize.x > pixelSize.y)
            {
                vector = new Vector3(1, pixelSize.y / pixelSize.x, 1);
            } else if (pixelSize.x < pixelSize.y)
            {
                vector = new Vector3(pixelSize.x/pixelSize.y,1,1);
            }
            else
            {
                vector = pixelSize.x >= 32 ? Vector3.one : new Vector3(pixelSize.x/32,pixelSize.y/32,1);
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
            Vector2 adjustedSpriteSize = sprite.bounds.size;
            if (Mathf.Approximately(adjustedSpriteSize.x, adjustedSpriteSize.y)) {
                return new Vector2(adjustedSpriteSize.x*32,adjustedSpriteSize.x*32);
            }
            if (adjustedSpriteSize.x > adjustedSpriteSize.y) {
                return new Vector2(32,adjustedSpriteSize.y/adjustedSpriteSize.x*32);
            }
            if (adjustedSpriteSize.y > adjustedSpriteSize.x) {
                return new Vector2(adjustedSpriteSize.x/adjustedSpriteSize.y*32,32);
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

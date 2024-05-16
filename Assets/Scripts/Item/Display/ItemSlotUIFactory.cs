using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items.Tags;
using TMPro;

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

        public static string formatAmountText(int amount,bool oneInvisible = true) {
            if (amount == 1 && oneInvisible) {
                return "";
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
    }
    public static class ItemSlotUIFactory
    {
        

        public static ItemSlotUI newItemSlotUI(ItemSlot itemSlot, Transform parent, Color? color, bool enableText = true,  string suffix = null) {
            GameObject slot = new GameObject();
            ItemSlotUI itemSlotUI = slot.AddComponent<ItemSlotUI>();
            slot.transform.SetParent(parent);
            itemSlotUI.init(color,enableText);
            if (suffix == null) {
                slot.name = ItemDisplayUtils.SlotName;
            } else {
                slot.name = ItemDisplayUtils.SlotName + suffix;
            }
            itemSlotUI.display(itemSlot);
            return itemSlotUI;
        }

        public static List<ItemSlotUI> getSlotsForInventory(List<ItemSlot> inventories, Transform parent, Color? color, bool enableText = true) {
            List<ItemSlotUI> slots = new List<ItemSlotUI>();
            for (int i = 0; i < inventories.Count; i++) {
                ItemSlotUI slot = newItemSlotUI(inventories[i],parent, color,enableText:enableText,suffix: i.ToString());
                slots.Add(slot);
                slot.transform.SetParent(parent);
            }
            return slots;
        }


        

        
        

        
        

       
        

        
        /*
    
        public static void reload(GameObject slot, ItemSlot itemSlot) {
            if (slot == null) {
                return;
            }
            if (itemSlot == null || itemSlot.itemObject == null) {
                return;   
            }
            Transform imageTransform = slot.transform.Find(itemImageName);
            if (imageTransform == null) {
                getItemImage(itemSlot,slot.transform);
            } else {
                Sprite sprite = itemSlot.itemObject.getSprite();
                Image itemImage = imageTransform.GetComponent<Image>();
                if (itemImage.sprite != sprite) {
                    itemImage.sprite = sprite;
                    imageTransform.GetComponent<RectTransform>().sizeDelta = getItemSize(sprite);
                }
            }
            Transform numberTransform = slot.transform.Find(itemAmountName);
            if (numberTransform == null) {
                getNumber(itemSlot,slot.transform);
            } else {
                numberTransform.GetComponent<TextMeshProUGUI>().text = formatAmountText(itemSlot.amount);
            }

        }

        public static void unload(Transform slotTransform) {
            Transform imageTransform = slotTransform.Find(itemImageName);
            if (imageTransform != null) {
                GameObject.Destroy(imageTransform.gameObject);
            }
            Transform amountTransform = slotTransform.Find(ItemAmountName);
            if (amountTransform != null) {
                GameObject.Destroy(amountTransform.gameObject);
            }
            Transform frontTagTransform = slotTransform.Find(ItemTagNameFront);
            if (frontTagTransform != null) {
                GameObject.Destroy(frontTagTransform.gameObject);
            }
            Transform behindTagTransform = slotTransform.Find(ItemTagNameBehind);
            if (behindTagTransform != null) {
                GameObject.Destroy(behindTagTransform.gameObject);
            }
        }

        public static void replaceAmountTextWithString(Transform slotTransform, string text) {
            Transform amountTransform = slotTransform.Find(ItemAmountName);
            if (amountTransform != null) {
                amountTransform.GetComponent<TextMeshProUGUI>().text = text;
            }
        }
        */
    }
    
}


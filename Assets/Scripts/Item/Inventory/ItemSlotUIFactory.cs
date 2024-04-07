using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemModule.Tags;
using TMPro;

namespace ItemModule {
    public static class ItemSlotUIFactory
    {
        private static string itemImageName = "item";
        private static string itemAmountName = "amount";
        private static string itemTagName = "tags";

        public static string ItemImageName { get => itemImageName; }
        public static string ItemAmountName { get => itemAmountName; }
        public static string ItemTagName { get => itemTagName; }
        private static readonly string[] suffixes = {"k","M","B","T"};

        public static GameObject getSlot(ItemSlot itemSlot) {
            GameObject slot = GlobalHelper.instantiateFromResourcePath("UI/SerializedItemSlot/SerializedItemSlotPanel");
            getItemImage(itemSlot,slot.transform);
            getNumber(itemSlot,slot.transform);
            getTagObject(itemSlot,slot.transform);
            return slot;
        }
        public static GameObject getItemImage(ItemSlot itemSlot, Transform parent) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return null;    
            }
            GameObject imageObject = getItemImage(itemSlot.itemObject);
            imageObject.transform.SetParent(parent,false);
            return imageObject;
        }

        public static GameObject getItemImage(ItemObject itemObject, bool scale = false) {
            GameObject imageObject = new GameObject();
            imageObject.name = itemImageName;
            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;
            imageObject.AddComponent<CanvasRenderer>();
            Image image = imageObject.AddComponent<Image>();
            image.sprite = itemObject.getSprite();
            rectTransform.sizeDelta = getItemSize(image.sprite);
            return imageObject;
        }

        public static GameObject getItemImage(ItemObject itemObject, Transform parent) {
            GameObject item = getItemImage(itemObject);
            item.transform.SetParent(parent,false);
            return item;
        }
        public static GameObject getItemImage(ItemSlot itemSlot) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return null;    
            }
            return getItemImage(itemSlot.itemObject);
        }

        public static GameObject getTagObject(ItemSlot itemSlot, Transform parent) {
            if (itemSlot == null) {
                return null;
            }
            if (itemSlot.tags == null || itemSlot.tags.Dict == null) {
                return null;
            }
            GameObject tagObject = new GameObject();
            tagObject.name = itemTagName;
            tagObject.transform.SetParent(parent,false);
            tagObject.transform.SetSiblingIndex(0);
            setItemImageTagVisuals(itemSlot,tagObject);
            return tagObject;
        }

        public static void setItemImageTagVisuals(ItemSlot itemSlot, GameObject itemImageObject) {
            if (itemSlot.tags == null || itemSlot.tags.Dict == null) {
                return;
            }
            
            foreach (KeyValuePair<ItemTag, object> keyValuePair in itemSlot.tags.Dict) {
                GameObject visualElement = keyValuePair.Key.getVisualElement(itemSlot,keyValuePair.Value);
                if (visualElement == null) {
                    continue;
                }
                visualElement.transform.SetParent(itemImageObject.transform,false);
            }
            
        }

        public static GameObject getNumber(ItemSlot itemSlot, Transform parent) {
            if (itemSlot == null || itemSlot.itemObject == null) {
                return null;    
            }
            GameObject number = new GameObject();
            number.name = itemAmountName;
            number.transform.SetParent(parent,false);
            number.AddComponent<RectTransform>();
            TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();

            textMeshPro.text = formatAmountText(itemSlot.amount);
            
            textMeshPro.fontSize = 25;
            RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1,1);
            rectTransform.anchorMin = new Vector2(0,0);
            rectTransform.localPosition = new Vector3(2,-2,1);
            rectTransform.sizeDelta = Vector2.zero;
            textMeshPro.alignment = TextAlignmentOptions.BottomRight;
            return number;
        }

        private static string formatAmountText(int amount) {
            if (amount < 1000) {
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

        public static Vector2 getItemScale(Sprite sprite) {
            Vector2 size = getItemSize(sprite);
            if (size.x >= size.y) {
                return new Vector2(1,size.y/size.x);
            } else {
                return new Vector2(size.x/size.y,1);
            }
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
            
            
            Transform tagTransform = slot.transform.Find(itemTagName);
            if (tagTransform == null) {
                getTagObject(itemSlot,slot.transform);
            } else {
                // TODO
            }
        }

        public static void load(ItemSlot itemSlot,Transform transform) {
            ItemSlotUIFactory.getItemImage(itemSlot,transform);
            ItemSlotUIFactory.getNumber(itemSlot,transform);
            ItemSlotUIFactory.getTagObject(itemSlot,transform);
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
            Transform tagTransform = slotTransform.Find(ItemTagName);
            if (tagTransform != null) {
                GameObject.Destroy(tagTransform.gameObject);
            }
        }
    }
    
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemModule.Tags;
using TMPro;

namespace ItemModule {
    public static class ItemSlotUIFactory
    {
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
            imageObject.name = "item";
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
            tagObject.name = "tags";
            tagObject.transform.SetParent(parent,false);
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
            number.name = "amount";
            number.transform.SetParent(parent,false);
            number.AddComponent<RectTransform>();
            TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = itemSlot.amount.ToString();
            textMeshPro.fontSize = 30;
            RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(5f,5f,1);
            rectTransform.sizeDelta = new Vector2(96,96);
            textMeshPro.alignment = TextAlignmentOptions.BottomLeft;
            return number;
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
    }
}


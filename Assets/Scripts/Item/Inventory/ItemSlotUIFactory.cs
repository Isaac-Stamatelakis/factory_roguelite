using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items.Tags;
using TMPro;

namespace Items.Inventory {
    public static class ItemSlotUIFactory
    {
        private static string slotName = "slot";
        private static string itemImageName = "item";
        private static string itemAmountName = "amount";
        private static string itemTagNameFront = "tagFront";
        private static string itemTagNameBehind = "tagBehind";
        public static string ItemImageName { get => itemImageName; }
        public static string ItemAmountName { get => itemAmountName; }
        public static string ItemTagNameFront { get => itemTagNameFront; }
        public static string ItemTagNameBehind { get => itemTagNameBehind; }
        private static readonly string[] suffixes = {"k","M","B","T"};

        public static GameObject getSlot(ItemSlot itemSlot, int index) {
            GameObject slot = GlobalHelper.instantiateFromResourcePath(InventoryHelper.SolidSlotPrefabPath);
            slot.name = slotName + index;
            if (itemSlot != null && itemSlot.itemObject != null) {
                getItemImage(itemSlot,slot.transform);
                getNumber(itemSlot,slot.transform);
                getTagObject(itemSlot,slot.transform);
            }
            return slot;
        }

        public static void getSlotsForInventory(List<ItemSlot> inventories, Transform container) {
            for (int i = 0; i < inventories.Count; i++) {
                GameObject slot = getSlot(inventories[i],i);
                slot.transform.SetParent(container);
            }
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
            
            GameObject endTag = new GameObject();
            endTag.name = itemTagNameBehind;
            endTag.transform.SetParent(parent,false);
            endTag.transform.SetSiblingIndex(0);

            GameObject frontTag = new GameObject();
            frontTag.name = itemTagNameFront;
            frontTag.transform.SetParent(parent,false);
            
            setItemImageTagVisuals(itemSlot,frontTag.transform,endTag.transform);
            return frontTag;
        }

        public static void reloadTagVisual(ItemSlot itemSlot, Transform frontTag, Transform endTag) {
            if (itemSlot == null) {
                return;
            }
            if (itemSlot.tags == null || itemSlot.tags.Dict == null) {
                return;
            }
            setItemImageTagVisuals(itemSlot,frontTag.transform,endTag.transform);
        }

        public static void setItemImageTagVisuals(ItemSlot itemSlot, Transform frontContainer, Transform endContainer) {
            if (itemSlot.tags == null || itemSlot.tags.Dict == null) {
                return;
            }
            
            foreach (KeyValuePair<ItemTag, object> keyValuePair in itemSlot.tags.Dict) {
                ItemTag tag = keyValuePair.Key;
                object data = keyValuePair.Value;
                GameObject visualElement = tag.getVisualElement(itemSlot,data);
                if (visualElement == null) {
                    continue;
                }
                bool inFront = tag.getVisualLayer();
                if (inFront) {
                    visualElement.transform.SetParent(frontContainer,false);
                } else {
                    visualElement.transform.SetParent(endContainer,false);
                }
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
            
            textMeshPro.fontSize = 20;
            RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1,1);
            rectTransform.anchorMin = new Vector2(0,0);
            rectTransform.localPosition = new Vector3(2,-2,1);
            rectTransform.sizeDelta = Vector2.zero;
            textMeshPro.alignment = TextAlignmentOptions.BottomRight;
            return number;
        }
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
            
            /*
            Transform frontTag = slot.transform.Find(itemTagNameFront);
            Transform endTag = slot.transform.Find(itemTagNameBehind);
            if (frontTag == null && endTag == null) { // This is slightly unsafe but these if both are either null or not null
                getTagObject(itemSlot,slot.transform);
            } else {
                GlobalHelper.deleteAllChildren(frontTag);
                GlobalHelper.deleteAllChildren(endTag);
                reloadTagVisual(itemSlot,frontTag,endTag);
            }
            */
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
    }
    
}


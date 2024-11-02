using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;

namespace Items {
    public class ItemSlotUI : MonoBehaviour
    {
        private Image itemSprite;
        private TextMeshProUGUI amountText;
        private Transform tagBehindContainer;
        private Transform tagFrontContainer;
        private int counter;
        private bool textEnabled;
        private ItemSlot displayed;
        public void FixedUpdate() {
            counter ++;
            // Semi work around
            if (displayed != null && displayed.itemObject != null && displayed.itemObject.getDisplayType() == ItemDisplayType.Animated) {
                displayItemSprite(displayed.itemObject);
            }
        }
        public void init(Color? color,bool textEnabled = true) {
            if (color != null) {
                Image panel = gameObject.GetComponent<Image>();
                if (panel == null) {
                    panel = gameObject.AddComponent<Image>();
                }
                panel.color = (Color) color;
            }
            if (gameObject.GetComponent<RectTransform>() == null) {
                gameObject.AddComponent<RectTransform>();
            }
            this.textEnabled = textEnabled;  
        }

        public void display(ItemSlot itemSlot) {
            this.displayed = itemSlot;
            if (itemSlot == null || itemSlot.itemObject == null) {
                if (transform.childCount != 0) {
                    unload();
                }
                return;
            } 
            createRequiredObjects(itemSlot);
            if (amountText != null) {
                amountText.text = ItemDisplayUtils.formatAmountText(itemSlot.amount);
            }
            displayItemSprite(itemSlot.itemObject);
        }
        public ItemSlot getDisplayedSlot() {
            return displayed;
        }

        public void unload() {
            GlobalHelper.deleteAllChildren(transform);
        }

        public void displayItemSprite(ItemObject itemObject) {
            switch (itemObject.getDisplayType()) {
                case ItemDisplayType.Single:
                    setImageSprite(itemSprite,itemObject.getSprite());
                    break;
                case ItemDisplayType.Stack:
                    Image[] images = itemSprite.GetComponentsInChildren<Image>();
                    Sprite[] stackSprites = itemObject.getSprites();
                    for (int i = 0; i < stackSprites.Length; i++) {
                        int imageIndex = stackSprites.Length-i; // Sprites are orded by index with larger showing lower
                        setImageSprite(images[imageIndex],stackSprites[i]);
                    }
                    break;
                case ItemDisplayType.Animated:
                    Sprite[] animationSprites = itemObject.getSprites();
                    int adjustedCounter = Mathf.FloorToInt(counter/(float) ItemDisplayUtils.AnimationSpeed);
                    int animationIndex = adjustedCounter % animationSprites.Length;
                    setImageSprite(itemSprite,animationSprites[animationIndex]);
                    break;
            }
        }

        public void displayText(string text) {
            if (amountText == null) {
                amountText = generateNumber();
            }
            amountText.text = text;
        }

        private void setImageSprite(Image image, Sprite sprite) {
            image.sprite = sprite;
            image.transform.localScale = ItemDisplayUtils.getItemScale(sprite);
        }


        private void createRequiredObjects(ItemSlot itemSlot) {
            if (itemSprite == null) {
                itemSprite = generateItemImage();
            }
            if (amountText == null && textEnabled) {
                amountText = generateNumber();
            }
            if (itemSlot.tags != null && itemSlot.tags.Dict != null) {
                if (tagBehindContainer == null && tagFrontContainer == null) {
                    (tagFrontContainer,tagBehindContainer) = generateTagContainers();
                }
            } else {
                if (tagFrontContainer != null) {
                    GameObject.Destroy(tagFrontContainer.gameObject);
                }
                if (tagBehindContainer != null) {
                    GameObject.Destroy(tagBehindContainer.transform);
                }
            }
            if (itemSlot.itemObject.getDisplayType() == ItemDisplayType.Stack) {
                itemSprite.enabled = false;
                int requiredImages = itemSlot.itemObject.getSprites().Length;
                while (itemSprite.transform.childCount < requiredImages) {
                    GameObject imageObject = new GameObject();
                    imageObject.name = ItemDisplayUtils.StackSpriteImageName + itemSprite.transform.childCount;
                    imageObject.AddComponent<Image>();
                    imageObject.transform.SetParent(itemSprite.transform,false);
                    RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                }
                while (itemSprite.transform.childCount > requiredImages) {
                    Transform child = itemSprite.transform.GetChild(itemSprite.transform.childCount-1);
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        public Image generateItemImage() {
            GameObject imageObject = new GameObject();
            imageObject.name = ItemDisplayUtils.ItemImageName;
            imageObject.transform.SetParent(transform,false);
            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            imageObject.AddComponent<CanvasRenderer>();
            Image image = imageObject.AddComponent<Image>();
            return image;
        }
        public TextMeshProUGUI generateNumber() {
            GameObject number = new GameObject();
            number.name = ItemDisplayUtils.ItemAmountName;
            number.transform.SetParent(transform,false);
            number.AddComponent<RectTransform>();
            TextMeshProUGUI textMeshPro = number.AddComponent<TextMeshProUGUI>();
            textMeshPro.fontSize = 20;
            RectTransform rectTransform = textMeshPro.GetComponent<RectTransform>();
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.localPosition = new Vector3(2,-2,1);
            rectTransform.sizeDelta = Vector2.zero;
            textMeshPro.alignment = TextAlignmentOptions.BottomRight;
            return textMeshPro;
        }
        public (Transform,Transform) generateTagContainers() {
            GameObject endTag = new GameObject();
            endTag.name = ItemDisplayUtils.ItemTagNameBehind;
            endTag.transform.SetParent(transform,false);
            endTag.transform.SetSiblingIndex(0);

            GameObject frontTag = new GameObject();
            frontTag.name = ItemDisplayUtils.ItemTagNameFront;
            frontTag.transform.SetParent(transform,false);
            return (frontTag.transform,endTag.transform);
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
        
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;
using Items.Transmutable;

namespace Items {
    public class ItemSlotUI : MonoBehaviour
    {
        private Image itemImage;
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
                ItemDisplayUtils.DisplayItemSprite(itemImage, displayed.itemObject, counter);
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
                    Unload();
                }
                return;
            }
            CreateRequiredObjects(itemSlot);
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                itemImage.color = transmutableItemObject.getMaterial().color;
            }
            else
            {
                itemImage.color = Color.white;
            }
            if (amountText != null) {
                amountText.text = ItemDisplayUtils.formatAmountText(itemSlot.amount);
            }
            ItemDisplayUtils.DisplayItemSprite(itemImage, itemSlot.itemObject, counter);
        }
        public ItemSlot GetDisplayedSlot() {
            return displayed;
        }

        public void Unload() {
            GlobalHelper.deleteAllChildren(transform);
        }

        public void DisplayText(string text) {
            if (amountText == null) {
                amountText = GenerateNumber();
            }
            amountText.text = text;
        }

        private void CreateRequiredObjects(ItemSlot itemSlot) {
            if (itemImage == null) {
                itemImage = GenerateItemImage();
            }
            if (amountText == null && textEnabled) {
                amountText = GenerateNumber();
            }
            if (itemSlot.tags != null && itemSlot.tags.Dict != null) {
                if (tagBehindContainer == null && tagFrontContainer == null) {
                    (tagFrontContainer,tagBehindContainer) = GenerateTagContainers();
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
                itemImage.enabled = false;
                int requiredImages = itemSlot.itemObject.getSprites().Length;
                while (itemImage.transform.childCount < requiredImages) {
                    GameObject imageObject = new GameObject();
                    imageObject.name = ItemDisplayUtils.StackSpriteImageName + itemImage.transform.childCount;
                    imageObject.AddComponent<Image>();
                    imageObject.transform.SetParent(itemImage.transform,false);
                    RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = Vector2.zero;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                }
                while (itemImage.transform.childCount > requiredImages) {
                    Transform child = itemImage.transform.GetChild(itemImage.transform.childCount-1);
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        public Image GenerateItemImage() {
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
        public TextMeshProUGUI GenerateNumber() {
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
        public (Transform,Transform) GenerateTagContainers() {
            GameObject endTag = new GameObject();
            endTag.name = ItemDisplayUtils.ItemTagNameBehind;
            endTag.transform.SetParent(transform,false);
            endTag.transform.SetSiblingIndex(0);

            GameObject frontTag = new GameObject();
            frontTag.name = ItemDisplayUtils.ItemTagNameFront;
            frontTag.transform.SetParent(transform,false);
            return (frontTag.transform,endTag.transform);
        }

        public static void SetItemImageTagVisuals(ItemSlot itemSlot, Transform frontContainer, Transform endContainer) {
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


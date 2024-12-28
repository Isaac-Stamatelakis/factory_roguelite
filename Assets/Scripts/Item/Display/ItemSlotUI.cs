using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;
using Items.Transmutable;
using UnityEngine.Serialization;

namespace Items {
    public struct ItemDisplay
    {
        public Sprite Sprite;
        public Color Color;

        public ItemDisplay(Sprite sprite = null, Color color = default)
        {
            Sprite = sprite;
            Color = color;
        }
    }
    
    public class ItemSlotUI : MonoBehaviour
    {
        public Image Panel;
        public Image ItemImage;
        public TextMeshProUGUI AmountText;
        public Transform TagBehindContainer;
        public Transform TagFrontContainer;
        private ItemDisplay[] toDisplay;
        private int counter;
        private ItemSlot displayedSlot;
        public void FixedUpdate() {
            if (toDisplay == null || toDisplay.Length == 0)
            {
                return;
            }
            counter ++;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            int index = (counter/ItemDisplayUtils.AnimationSpeed) % toDisplay.Length;
            ItemDisplay display = toDisplay[index];
            ItemDisplayUtils.SetImageItemSprite(ItemImage, display.Sprite);
            ItemImage.color = display.Color;
        }

        public void SetPanelColor(Color color)
        {
            Panel.color = color;
        }

        public void SetText(string text)
        {
            AmountText.text = text;
        }

        public void RefreshAmountText()
        {
            if (displayedSlot == null) return;
            AmountText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount);
        }
        public void Display(ItemSlot itemSlot)
        {
            if (itemSlot == null || itemSlot.itemObject == null) {
                Unload();
                return;
            }
            Sprite[] sprites = itemSlot.itemObject.getSprites();
            if (sprites == null || sprites.Length == 0)
            {
                Unload();
                return;
            }
            displayedSlot = itemSlot;
            
            toDisplay = new ItemDisplay[sprites.Length];
            Color color = itemSlot.itemObject is TransmutableItemObject transmutableItemObject ? transmutableItemObject.getMaterial().color : Color.white;
            for (int i = 0; i < toDisplay.Length; i++)
            {
                toDisplay[i] = new ItemDisplay(sprites[i], color);
            }

            counter = 0;
            RefreshDisplay();
            ItemImage.gameObject.SetActive(true);
            AmountText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount);
            DisplayTagVisuals(itemSlot);
        }

        public void Display(ItemDisplay[] itemsToDisplay)
        {
            ItemImage.gameObject.SetActive(true);
            toDisplay = itemsToDisplay;
            displayedSlot = null;
            DisableItemSlotVisuals();
            counter = 0;
            RefreshDisplay();
        }
        
        public ItemSlot GetDisplayedSlot() {
            return displayedSlot;
        }

        public void DisableItemSlotVisuals()
        {
            GlobalHelper.deleteAllChildren(TagBehindContainer);
            GlobalHelper.deleteAllChildren(TagFrontContainer);
            AmountText.text = "";
        }
        public void Unload()
        {
            toDisplay = null;
            ItemImage.gameObject.SetActive(false);
            DisableItemSlotVisuals();
        }
        
        /*
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
        */

        public void DisplayTagVisuals(ItemSlot itemSlot) {
            if (itemSlot.tags?.Dict == null) {
                return;
            }
            
            foreach (var keyValuePair in itemSlot.tags.Dict) {
                ItemTag itemTag = keyValuePair.Key;
                object data = keyValuePair.Value;
                GameObject visualElement = itemTag.getVisualElement(itemSlot,data);
                if (visualElement == null) {
                    continue;
                }
                bool inFront = itemTag.getVisualLayer();
                visualElement.transform.SetParent(inFront ? TagFrontContainer : TagBehindContainer, false);
            }
        }
        
    }
}


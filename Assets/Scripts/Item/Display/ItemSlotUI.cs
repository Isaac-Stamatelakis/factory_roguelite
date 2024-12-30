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
            if (ReferenceEquals(displayedSlot?.itemObject,null) || displayedSlot.amount == 0)
            {
                AmountText.text = string.Empty;
                ItemImage.gameObject.SetActive(false);
                return;
            }
            AmountText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount);
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
        
        public void Display(ItemSlot itemSlot)
        {
            if (ReferenceEquals(itemSlot?.itemObject,null)) {
                if (ItemImage.IsActive()) Unload();
                displayedSlot = null;
                return;
            }

            if (!ReferenceEquals(displayedSlot?.itemObject, null) &&
                itemSlot.itemObject.id == displayedSlot.itemObject.id)
            {
                return;
            }
            Sprite[] sprites = itemSlot.itemObject.getSprites();
            if (sprites == null || sprites.Length == 0)
            {
                Unload();
                return;
            }
            
            displayedSlot = itemSlot;
            AmountText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount);
            toDisplay = new ItemDisplay[sprites.Length];
            Color color = itemSlot.itemObject is TransmutableItemObject transmutableItemObject ? transmutableItemObject.getMaterial().color : Color.white;
            for (int i = 0; i < toDisplay.Length; i++)
            {
                toDisplay[i] = new ItemDisplay(sprites[i], color);
            }

            counter = 0;
            RefreshDisplay();
            ItemImage.gameObject.SetActive(true);
            
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


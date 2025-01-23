using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;
using Items.Transmutable;
using UnityEngine.Serialization;

namespace Items {
    public class ItemDisplayList
    {
        public ItemDisplay[] Elements;
        public int RefreshRate;

        public ItemDisplayList(ItemDisplay[] elements, int refreshRate)
        {
            Elements = elements;
            RefreshRate = refreshRate;
        }

        public ItemDisplay GetItemToDisplay(int counter)
        {
            int index = (counter/RefreshRate) % Elements.Length;
            return Elements[index];
        }
        
    }
    public class ItemDisplay
    {
        public Sprite Sprite;
        public Color Color;

        public ItemDisplay(Sprite sprite, Color color)
        {
            Sprite = sprite;
            Color = color;
        }
    }
    
    public class ItemSlotUI : MonoBehaviour
    {
        public Image Panel;
        public Image ItemImage;
        [FormerlySerializedAs("AmountText")] public TextMeshProUGUI mBottomText;
        public TextMeshProUGUI mTopText;
        public Transform TagBehindContainer;
        public Transform TagFrontContainer;
        private ItemDisplayList currentDisplayList;
        
        private int counter;
        private ItemSlot displayedSlot;
        
        public void FixedUpdate() {
            if (currentDisplayList == null)
            {
                return;
            }
            counter ++;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            bool displayingItemSlot = displayedSlot != null;
            if (displayingItemSlot)
            {
                if (ItemSlotUtils.IsItemSlotNull(displayedSlot))
                {
                    mBottomText.text = string.Empty;
                    ItemImage.gameObject.SetActive(false);
                    return;
                }
                SetAmountText();
            }

            if (currentDisplayList == null)
            {
                mBottomText.text = string.Empty;
                ItemImage.gameObject.SetActive(false);
                return;
            }
            
            ItemDisplay display = currentDisplayList.GetItemToDisplay(counter);
            ItemDisplayUtils.SetImageItemSprite(ItemImage, display.Sprite);
            ItemImage.color = display.Color;
        }

        public void SetPanelColor(Color color)
        {
            Panel.color = color;
        }
        
        public virtual void SetAmountText()
        {
            mBottomText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount);
        }
        
        public void Display(ItemSlot itemSlot)
        {
            if (ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
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
            
            var toDisplay = new ItemDisplay[sprites.Length];
            Color color = itemSlot.itemObject is TransmutableItemObject transmutableItemObject ? transmutableItemObject.getMaterial().color : Color.white;
            for (int i = 0; i < toDisplay.Length; i++)
            {
                toDisplay[i] = new ItemDisplay(sprites[i], color);
            }

            currentDisplayList = new ItemDisplayList(toDisplay, ItemDisplayUtils.AnimationSpeed);
            counter = 0;
            RefreshDisplay();
            ItemImage.gameObject.SetActive(true);
            
            DisplayTagVisuals(itemSlot);
        }

        public void Display(ItemSlot itemSlot, string topText)
        {
            Display(itemSlot);
            mTopText.text = topText;
        }

        public void SetTopText(string topText)
        {
            if (ReferenceEquals(mTopText, null))
            {
                Debug.LogWarning($"Attempted to set top text on ItemSlotUI '{name}' which does not contain a top text element.");
                return;
            }
            mTopText.text = topText;
        }
        
        
        public ItemSlot GetDisplayedSlot() {
            return displayedSlot;
        }

        public void DisableItemSlotVisuals()
        {
            GlobalHelper.deleteAllChildren(TagBehindContainer);
            GlobalHelper.deleteAllChildren(TagFrontContainer);
            mBottomText.text = "";
            if (!ReferenceEquals(mTopText,null)) mTopText.text = "";
     
        }
        public void Unload()
        {
            currentDisplayList = null;
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
                if (ReferenceEquals(visualElement,null)) continue;
                
                bool inFront = itemTag.getVisualLayer();
                visualElement.transform.SetParent(inFront ? TagFrontContainer : TagBehindContainer, false);
            }
        }
        
    }
}


using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;
using Items.Transmutable;
using Unity.VisualScripting;
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
        private enum ItemAnimationSpeed
        {
            VerySlow = -1,
            Slow = 0,
            Normal = 1,
            Fast = 2,
            VeryFast = 3,
        }
        public ItemState ItemState = ItemState.Solid;
        public Image Panel;
        
        public Image ItemImage;
        [FormerlySerializedAs("AmountText")] public TextMeshProUGUI mBottomText;
        public TextMeshProUGUI mTopText;
        public Transform TagBehindContainer;
        public Transform TagFrontContainer;
        [SerializeField] private ItemAnimationSpeed animationSpeed = ItemAnimationSpeed.Normal;
        public bool ScaleItems = true;
        private ItemDisplayList currentDisplayList;
        [NonSerialized] public bool Paused;
        private bool staticTopText;
        public bool StaticTopText
        {
            get => staticTopText; set => staticTopText = value;
        }
        
        private int counter;
        private ItemSlot displayedSlot;
        
        public void FixedUpdate() {
            if (currentDisplayList == null || Paused)
            {
                return;
            }
            counter ++;
            RefreshDisplay();
        }

        public void DisplayFirstFrame()
        {
            if (displayedSlot == null) return;
            counter = 0;
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
            
            
            DisplayItem(currentDisplayList,ItemImage);
        }

        private void DisplayItem(ItemDisplayList itemDisplayList, Image image)
        {
            ItemDisplay display = itemDisplayList.GetItemToDisplay(counter);
            if (ScaleItems)
            {
                ItemDisplayUtils.SetImageItemSprite(image, display.Sprite);
            }
            else
            {
                image.sprite = display.Sprite;
            }
            
            image.color = display.Color;
        }

        public void SetPanelColor(Color color)
        {
            Panel.color = color;
        }
        
        public virtual void SetAmountText()
        {
            mBottomText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount,itemState:ItemState);
        }
        
        public void Display(ItemSlot itemSlot)
        {
            if (Paused) return;
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
            GlobalHelper.DeleteAllChildren(ItemImage.transform);
            displayedSlot = itemSlot;
            
            var toDisplay = new ItemDisplay[sprites.Length];
            Color color = Color.white;
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                color = transmutableItemObject.getMaterial().color;
                if (transmutableItemObject.getMaterial().OverlaySprite)
                {
                    AddOverlay(transmutableItemObject.getMaterial().OverlaySprite, Color.white,$"TransmutableOverlay");
                }
            }
            if (itemSlot.itemObject is TileItem tileItem)
            {
                if (tileItem.tileOptions?.Overlay)
                {
                    var overlay = tileItem.tileOptions.Overlay;
                    Sprite tileSprite = TileItem.GetDefaultSprite(overlay.GetDisplayTile());
                    AddOverlay(tileSprite, overlay.GetColor(),$"TileOverlay");
                }

                if (tileItem.tileOptions?.TileColor)
                {
                    color = tileItem.tileOptions.TileColor.GetColor();
                }
            }
            
            
            for (var index = 0; index < itemSlot.itemObject.SpriteOverlays?.Length; index++)
            {
                var spriteOverlay = itemSlot.itemObject.SpriteOverlays[index];
                AddOverlay(spriteOverlay.Sprite, spriteOverlay.Color,$"SpriteOverlay{index}");
            }

            for (int i = 0; i < toDisplay.Length; i++)
            {
                toDisplay[i] = new ItemDisplay(sprites[i], color);
            }
            
            currentDisplayList = new ItemDisplayList(toDisplay, GetAnimationSpeedValue());
            counter = 0;
            RefreshDisplay();
            ItemImage.enabled = ItemImage.sprite;
            ItemImage.gameObject.SetActive(true);
            
            DisplayTagVisuals(itemSlot);
        }

        private int GetAnimationSpeedValue()
        {
            switch (animationSpeed)
            {
                case ItemAnimationSpeed.VerySlow:
                    return 20;
                case ItemAnimationSpeed.Slow:
                    return 15;
                case ItemAnimationSpeed.Normal:
                    return 10;
                case ItemAnimationSpeed.Fast:
                    return 6;
                case ItemAnimationSpeed.VeryFast:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AddOverlay(Sprite sprite, Color color, string overlayName)
        {
            GameObject overlayObject = new GameObject(overlayName);
            Image overlayImage = overlayObject.gameObject.AddComponent<Image>();
            overlayImage.raycastTarget = false;
            overlayImage.sprite = sprite;
            overlayImage.color = color;
            RectTransform rectTransform = (RectTransform)overlayObject.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            overlayObject.transform.SetParent(ItemImage.transform, false);
        }
        
        public void Display(ItemSlot itemSlot, string topText)
        {
            Display(itemSlot);
            if (!staticTopText) mTopText.text = topText;
        }

        public void SetTopText(string topText)
        {
            if (staticTopText) return;
            if (ReferenceEquals(mTopText, null))
            {
                return;
            }
            mTopText.text = topText;
        }
        
        
        public ItemSlot GetDisplayedSlot() {
            return displayedSlot;
        }

        public void DisableItemSlotVisuals()
        {
            GlobalHelper.DeleteAllChildren(TagBehindContainer);
            GlobalHelper.DeleteAllChildren(TagFrontContainer);
            mBottomText.text = "";
            if (!staticTopText && !ReferenceEquals(mTopText,null)) mTopText.text = "";
     
        }
        public void Unload()
        {
            currentDisplayList = null;
            GlobalHelper.DeleteAllChildren(ItemImage.transform);
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
                GameObject visualElement = itemTag.GetUITagElement(itemSlot,data);
                if (ReferenceEquals(visualElement,null)) continue;

                ItemTagVisualLayer visualLayer = itemTag.GetVisualLayer();
                visualElement.transform.SetParent(visualLayer == ItemTagVisualLayer.Front ? TagFrontContainer : TagBehindContainer, false);
            }
        }
        
    }
}


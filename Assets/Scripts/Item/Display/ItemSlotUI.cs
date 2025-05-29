using System;
using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using Item.Transmutation;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items.Inventory;
using Items.Tags;
using Items.Transmutable;
using Tiles.Options.Overlay;
using Unity.VisualScripting;
using UnityEngine.Serialization;

namespace Items {
    public class ItemSlotUI : MonoBehaviour
    {
        private class AnimatedItemDisplay
        {
            public Image Image;
            public Sprite[] Sprites;
        }
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
        private readonly List<AnimatedItemDisplay> animatedItemDisplays = new();
        [NonSerialized] public bool Paused;
        private bool lockTopText;
        private bool lockBottomText;
        private bool displaying;
        public bool LockTopText
        {
            get => lockTopText; set => lockTopText = value;
        }

        public bool LockBottomText
        {
            get => lockBottomText; set => lockBottomText = value;
        }
        
        private int counter;
        protected ItemSlot displayedSlot;
        private int animationSpeedValue;
        
        public void FixedUpdate() {
            if (Paused) return;
            counter ++;
            if (!displaying) return;
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
            if (ItemSlotUtils.IsItemSlotNull(displayedSlot))
            {
                Unload();
                return;
            }
            SetAmountText();
            
            foreach (AnimatedItemDisplay animatedItemDisplay in animatedItemDisplays)
            {
                int index = counter / animationSpeedValue % animatedItemDisplay.Sprites.Length;
                SetImageSprite(animatedItemDisplay.Image,animatedItemDisplay.Sprites[index]);
            }
        }

        private void SetImageSprite(Image image, Sprite sprite)
        {
            if (ScaleItems)
            {
                ItemDisplayUtils.SetImageItemSprite(image, sprite);
            }
            else
            {
                image.sprite = sprite;
            }
        }
        

        public void SetPanelColor(Color color)
        {
            Panel.color = color;
        }
        
        public virtual void SetAmountText()
        {
            if (lockBottomText) return;
            mBottomText.text = ItemDisplayUtils.FormatAmountText(displayedSlot.amount,itemState:ItemState);
        }

        public void DisplayBottomText(string text)
        {
            if (lockBottomText) return;
            mBottomText.text = text;
        }
        
        public void Display(ItemSlot itemSlot)
        {
            if (Paused) return;
            if (ItemSlotUtils.IsItemSlotNull(itemSlot))
            {
                Unload();
                return;
            }
            
            if (displaying && ItemSlotUtils.AreEqual(itemSlot, displayedSlot))
            {
                this.displayedSlot = itemSlot;
                SetAmountText();
                return;
            }
            
            if (!ItemImage) return;
          
            ItemImage.gameObject.SetActive(true);
            displaying = true;
            displayedSlot = itemSlot;
            
            Sprite[] sprites = itemSlot.itemObject.GetSprites();
            if (sprites != null && sprites.Length > 0)
            {
                animationSpeedValue = GetAnimationSpeedValue();
                GlobalHelper.DeleteAllChildren(ItemImage.transform);
                animatedItemDisplays.Clear();
                SetImageSprite(ItemImage, sprites[0]);
                if (sprites.Length > 1)
                {
                    animatedItemDisplays.Add(new AnimatedItemDisplay
                    {
                        Image = ItemImage,
                        Sprites = sprites
                    });
                }
            }
            else
            {
                ItemImage.sprite = null;
            }
            
            ItemImage.color = itemSlot.itemObject is IColorableItem colorableItem ? colorableItem.Color : Color.white;
            ItemImage.material = null;
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                var transmutableMaterial = transmutableItemObject.getMaterial();
                if (transmutableMaterial)
                {
                    if (transmutableMaterial.OverlaySprite)
                    {
                        AddOverlay(transmutableItemObject.getMaterial().OverlaySprite, Color.white,$"TransmutableOverlay",null);
                    }

                    if (transmutableMaterial.ShaderMaterial)
                    {
                        ItemImage.material = transmutableMaterial.ShaderMaterial.UIMaterial;
                    }
                }
            } else if (itemSlot.itemObject is TileItem tileItem)
            {
                var tileOverlay = tileItem.tileOptions?.Overlay;
                if (tileOverlay)
                {
                    Sprite tileSprite = TileItem.GetDefaultSprite(tileOverlay.GetDisplayTile());
                    Material material = tileOverlay is IShaderTileOverlay shaderTileOverlay ? shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.UI) : null;
                    AddOverlay(tileSprite, tileOverlay.GetColor(),$"TileOverlay",material);
                }
            }
            
            
            for (var index = 0; index < itemSlot.itemObject.SpriteOverlays?.Length; index++)
            {
                var spriteOverlay = itemSlot.itemObject.SpriteOverlays[index];
                AddOverlay(spriteOverlay.Sprite, spriteOverlay.Color,$"SpriteOverlay:{index}",null);
            }

            if (itemSlot.itemObject is IAnimatedOverlayItem animatedOverlayItem)
            {
                SpriteCollection[] spriteCollections = animatedOverlayItem.SpriteCollectionOverlays;
                if (spriteCollections != null)
                {
                    for (var index = 0; index < spriteCollections.Length; index++)
                    {
                        var spriteCollection = spriteCollections[index];
                        if (!spriteCollection || spriteCollection.Sprites.Length == 0) continue;

                        Image overlayImage = AddOverlay(spriteCollection.Sprites[0], Color.white, $"AnimatedSpriteOverlay:{index}", null);

                        animatedItemDisplays.Add(new AnimatedItemDisplay
                        {
                            Image = overlayImage,
                            Sprites = spriteCollection.Sprites
                        });
                    }
                }
            }
            RefreshDisplay();
            
            bool mainSpriteNotNull = ItemImage.sprite;
            if (!mainSpriteNotNull && ItemImage.transform.childCount > 0)
            {
                Image childImage = ItemImage.transform.GetChild(0).GetComponent<Image>();
                ItemImage.transform.localScale = ItemDisplayUtils.GetItemScale(childImage.sprite);
            }
            
            ItemImage.enabled = mainSpriteNotNull;
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

        private Image AddOverlay(Sprite sprite, Color color, string overlayName, Material material)
        {
            GameObject overlayObject = new GameObject(overlayName);
            Image overlayImage = overlayObject.gameObject.AddComponent<Image>();
            overlayImage.material = material;
            overlayImage.raycastTarget = false;
            overlayImage.sprite = sprite;
            overlayImage.color = color;
            RectTransform rectTransform = (RectTransform)overlayObject.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            
            overlayObject.transform.SetParent(ItemImage.transform, false);
            return overlayImage;
        }
        
        public void Display(ItemSlot itemSlot, string topText)
        {
            Display(itemSlot);
            if (!lockTopText) mTopText.text = topText;
        }

        public void SetTopText(string topText)
        {
            if (lockTopText) return;
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
        }
        public void Unload()
        {
            displaying = false;
            animatedItemDisplays.Clear();
            GlobalHelper.DeleteAllChildren(ItemImage.transform);
            ItemImage.gameObject.SetActive(false);
            DisableItemSlotVisuals();
            if (!lockTopText && !ReferenceEquals(mTopText,null)) mTopText.text = string.Empty;
            if (!lockBottomText) mBottomText.text = string.Empty;
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


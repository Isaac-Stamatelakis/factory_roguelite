using System;
using System.Collections.Generic;
using Item.Slot;
using Item.Tags.ItemTagManagers;
using Items;
using Items.Tags;
using Items.Transmutable;
using Tiles.Options.Overlay;
using UnityEngine;

namespace Item.Display
{
    
    public class ItemWorldDisplay : MonoBehaviour
    {
        private class WorldSpriteAnimator
        {
            public SpriteRenderer Renderer;
            public Sprite[] AnimatedSprites;

            public WorldSpriteAnimator(SpriteRenderer renderer, Sprite[] animatedSprites)
            {
                Renderer = renderer;
                AnimatedSprites = animatedSprites;
            }
            public WorldSpriteAnimator(Transform parent, Sprite[] animatedSprites)
            {
                GameObject gameObject = new GameObject("WorldSpriteAnimator");
                gameObject.transform.SetParent(parent,false);
                Renderer = gameObject.AddComponent<SpriteRenderer>();
                gameObject.transform.localPosition = new Vector3(0, 0, -0.01f);
                AnimatedSprites = animatedSprites;
            }
        }
        private List<WorldSpriteAnimator> spriteAnimators = new();
        private Sprite[] animateSprites;
        public void FixedUpdate()
        {
            if (itemSlot.amount == 0)
            {
                gameObject.SetActive(false);
            }
            foreach (WorldSpriteAnimator worldSpriteAnimator in spriteAnimators)
            {
                int index = (int)(Time.fixedTime*10) % worldSpriteAnimator.AnimatedSprites.Length;
                worldSpriteAnimator.Renderer.sprite = worldSpriteAnimator.AnimatedSprites[index];
            }
        }

        private SpriteRenderer spriteRenderer;
        private ItemSlot itemSlot;
        
        private Material defaultMaterial;

        public void Awake()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            defaultMaterial = spriteRenderer.material;
        }

        public void Display(ItemSlot displaySlot)
        {
            this.itemSlot = displaySlot;
            
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            Decorate();
        }
        private void Decorate()
        {
            Sprite[] itemSprites = itemSlot.itemObject.GetSprites();
            if (itemSprites != null && itemSprites.Length > 1)
            {
                spriteAnimators.Add(new WorldSpriteAnimator(spriteRenderer, itemSprites));
            }
            spriteRenderer.sprite = itemSlot.itemObject.GetSprite();
            spriteRenderer.material = defaultMaterial;
            spriteRenderer.color = itemSlot.itemObject is IColorableItem colorableItem ? colorableItem.Color : Color.white;
            
            if (itemSlot.itemObject is ITransmutableItem transmutableItem)
            {
                var material = transmutableItem.getMaterial();
                if (material)
                {
                    if (material.HasShaders)
                    {
                        spriteRenderer.material = ItemRegistry.GetInstance().GetTransmutationWorldMaterial(material);
                    }
                }
            }
            gameObject.transform.localScale = new Vector3(0.5f, 0.5f,1f);
            
            AddOverlays();
        }

        private void AddOverlays()
        {
            
            void AddOverlaySprite(Sprite sprite, Color color, Material material)
            {
                GameObject overlayContainer = new GameObject("SpriteOverlay");
                overlayContainer.tag = "SpriteOverlay";
                SpriteRenderer overlaySpriteRenderer = overlayContainer.AddComponent<SpriteRenderer>();
                overlaySpriteRenderer.sprite = sprite;
                overlaySpriteRenderer.color = color;
                if (material)
                {
                    overlaySpriteRenderer.material = material;
                }
                overlayContainer.transform.SetParent(transform,false);
                overlayContainer.transform.localPosition = new Vector3(0, 0, -0.1f);
            }
            
            void SpawnTagObjects()
            {
                foreach (var (itemTag, data) in itemSlot.tags.Dict)
                {
                    GameObject tagObject = itemTag.GetWorldTagElement(itemSlot,data);
                    if (!tagObject) continue;
                    ItemTagVisualLayer visualLayer = itemTag.GetVisualLayer();
                    tagObject.transform.SetParent(transform,false);
                    switch (visualLayer)
                    {
                        case ItemTagVisualLayer.Front:
                            tagObject.transform.SetAsFirstSibling();
                            break;
                        case ItemTagVisualLayer.Back:
                            tagObject.transform.SetAsLastSibling();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            
            if (itemSlot.tags?.Dict != null)
            {
                SpawnTagObjects();
            }

            if (itemSlot.itemObject is IAnimatedOverlayItem animatedOverlayItem)
            {
                SpriteCollection[] spriteCollections = animatedOverlayItem.SpriteCollectionOverlays;
                foreach (SpriteCollection spriteCollection in spriteCollections)
                {
                    if (spriteCollection.Sprites.Length == 0) continue;
                    spriteAnimators.Add(new WorldSpriteAnimator(transform, spriteCollection.Sprites));
                }
            }

            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                if (transmutableItemObject.getMaterial().OverlaySprite)
                {
                    AddOverlaySprite(transmutableItemObject.getMaterial().OverlaySprite,Color.white,null);
                }
            }
            if (itemSlot.itemObject is TileItem tileItem)
            {
                var tileOverlay = tileItem.tileOptions.Overlay;
                if (tileOverlay)
                {
                    var material = tileOverlay is IShaderTileOverlay shaderTileOverlay ? shaderTileOverlay.GetMaterial(IShaderTileOverlay.ShaderType.World) : null;
                    AddOverlaySprite(TileItem.GetDefaultSprite(tileOverlay.GetDisplayTile()),tileOverlay.GetColor(),material);
                }
                var transmutableMaterialOverride = tileItem.tileOptions.TransmutableColorOverride;
                if (transmutableMaterialOverride && transmutableMaterialOverride.HasShaders)
                {
                    spriteRenderer.material = ItemRegistry.GetInstance().GetTransmutationUIMaterial(transmutableMaterialOverride);
                }
            }

            if (itemSlot.itemObject.SpriteOverlays != null)
            {
                foreach (SpriteOverlay spriteOverlay in itemSlot.itemObject.SpriteOverlays)
                {
                    AddOverlaySprite(spriteOverlay.Sprite,spriteOverlay.Color,spriteOverlay.Material);
                }
            }
        }
    }
}

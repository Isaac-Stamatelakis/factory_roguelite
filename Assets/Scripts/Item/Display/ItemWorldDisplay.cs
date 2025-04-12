using System;
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
        private Sprite[] animateSprites;
        public void FixedUpdate()
        {
            if (itemSlot.amount == 0)
            {
                
                gameObject.SetActive(false);
            }
            if (animate)
            {
                int index = (int)(Time.fixedTime*10) % animateSprites.Length;
                spriteRenderer.sprite = animateSprites[index];
            }
        }

        private SpriteRenderer spriteRenderer;
        private ItemSlot itemSlot;
        private bool animate;
        private Material defaultMaterial;

        public void Awake()
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            defaultMaterial = spriteRenderer.material;
        }

        public void Display(ItemSlot displaySlot)
        {
            this.itemSlot = displaySlot;
            animate = false;
            if (ItemSlotUtils.IsItemSlotNull(itemSlot)) return;
            Decorate();
        }
        private void Decorate()
        {
            Sprite[] itemSprites = itemSlot.itemObject.getSprites();
            if (itemSprites.Length > 1)
            {
                animate = true;
                animateSprites = itemSprites;
            }
            spriteRenderer.sprite = itemSlot.itemObject.getSprite();
            spriteRenderer.material = defaultMaterial;
            if (itemSlot.itemObject is TransmutableItemObject transmutableItemObject)
            {
                var material = transmutableItemObject.getMaterial();
                if (material)
                {
                    spriteRenderer.color = transmutableItemObject.getMaterial().color;
                    if (material.WorldShaderMaterial)
                    {
                        spriteRenderer.material = material.WorldShaderMaterial;
                    }
                    
                }
                
            } else if (itemSlot.itemObject is TileItem tileItem && tileItem.tileOptions.TileColor)
            {
                spriteRenderer.color = tileItem.tileOptions.TileColor.GetColor();
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
            }

            if (itemSlot.itemObject.SpriteOverlays != null)
            {
                foreach (SpriteOverlay spriteOverlay in itemSlot.itemObject.SpriteOverlays)
                {
                    AddOverlaySprite(spriteOverlay.Sprite,spriteOverlay.Color,null);
                }
            }
        }
    }
}

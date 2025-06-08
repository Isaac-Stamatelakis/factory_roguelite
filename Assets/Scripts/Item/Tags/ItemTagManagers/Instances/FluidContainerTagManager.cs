using System;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Tags.FluidContainers;
using Items.Transmutable;
using UnityEngine;
using UnityEngine.UI;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class FluidContainerTagManager : ItemTagManager, IItemTagWorldViewable, IItemTagUIViewable, IToolTipTagViewable, IItemTagReferencedType, IItemTagStackable, IItemTagNullStackable
    {
        public override string Serialize(object obj)
        {
            return obj is not ItemSlot itemSlot ? null : ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public override object Deserialize(string data)
        {
            return ItemSlotFactory.DeserializeSlot(data);
        }

        public GameObject GetWorldTagObject(object obj, ItemObject containerObject)
        {
            if (obj is not ItemSlot fluidItemSlot || fluidItemSlot.itemObject is not FluidTileItem fluidTileItem) {
                return null;
            }

            if (ItemSlotUtils.IsItemSlotNull(fluidItemSlot)) return null;
            if (containerObject is not IFluidContainerData fluidContainer) {
                return null;
            }
            
            GameObject fluidGameObject = new GameObject();
            
            fluidGameObject.name = fluidItemSlot.itemObject.name;
            SpriteRenderer spriteRenderer = fluidGameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = fluidItemSlot.itemObject.GetSprite();
            
            Vector2 scale = fluidContainer.GetWorldFluidSpriteScale();
            fluidGameObject.transform.localScale = scale;
            
            var (material, color) = fluidTileItem.GetRendererValues();
            spriteRenderer.material = material;
            spriteRenderer.color = color;
            return fluidGameObject;
        }

        public GameObject GetUITagObject(object obj, ItemObject containerObject)
        {
            if (obj is not ItemSlot fluidItemSlot || fluidItemSlot.itemObject is not FluidTileItem fluidTileItem) {
                return null;
            }
            if (ItemSlotUtils.IsItemSlotNull(fluidItemSlot)) return null;
            
            if (containerObject is not IFluidContainerData fluidContainer) {
                return null;
            }  
            Vector2Int spriteSize = fluidContainer.GetFluidSpriteSize();
            if (spriteSize.Equals(Vector2Int.zero)) {
                return null;
            }
            GameObject fluidObject = new GameObject();
            Image image = fluidObject.AddComponent<Image>();
            image.sprite = fluidItemSlot.itemObject.GetSprite();
            
            var (material, color) = fluidTileItem.GetRendererValues();
            image.material = material;
            image.color = color;
     
            RectTransform rectTransform = fluidObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = spriteSize;
            return fluidObject;
        }
        

        public string GetToolTip(object obj)
        {
            if (obj is not ItemSlot fluidItem) return null;
            if (ItemSlotUtils.IsItemSlotNull(fluidItem)) return null;
            return $"Storing {ItemDisplayUtils.FormatAmountText(fluidItem.amount,false,ItemState.Fluid)} of {fluidItem.itemObject.name}\n";
        }

        public object CreateDeepCopy(object obj)
        {
            return obj is not ItemSlot fluidItem ? null : new ItemSlot(fluidItem.itemObject, fluidItem.amount, null);
        }

        public bool AreStackable(object first, object second)
        {
            if (first is not ItemSlot firstSlot) return false;
            if (second is not ItemSlot secondSlot) return false;
            return firstSlot.amount == secondSlot.amount && String.Equals(firstSlot.itemObject?.id, secondSlot.itemObject?.id);
        }

        public bool IsStackableWithNullObject(object nonNullObject)
        {
            if (nonNullObject is not ItemSlot itemSlot) return true;
            return ItemSlotUtils.IsItemSlotNull(itemSlot);
        }
    }
}

using System;
using Item.GrabbedItem;
using Item.Slot;
using Items;
using Items.Tags.FluidContainers;
using UnityEngine;
using UnityEngine.UI;

namespace Item.Tags.ItemTagManagers.Instances
{
    public class FluidContainerTagManager : ItemTagManager, IItemTagWorldViewable, IItemTagUIViewable, IToolTipTagViewable, IItemTagReferencedType, IItemTagStackable
    {

        public override string Serialize(object obj)
        {
            return obj is not ItemSlot itemSlot ? null : ItemSlotFactory.seralizeItemSlot(itemSlot);
        }

        public override object Deserialize(string data)
        {
            return ItemSlotFactory.DeserializeSlot(data);
        }

        public GameObject GetWorldTagObject(object obj)
        {
            // TODO
            return null;
        }

        public GameObject GetUITagObject(object obj)
        {
            if (obj is not ItemSlot fluidItem) {
                return null;
            }
            
            if (fluidItem.itemObject is not IFluidContainerData fluidContainer) {
                return null;
            }  
            Vector2Int spriteSize = fluidContainer.GetFluidSpriteSize();
            if (spriteSize.Equals(Vector2Int.zero)) {
                return null;
            }
            GameObject fluidObject = new GameObject();
            Image image = fluidObject.AddComponent<Image>();
            image.sprite = fluidItem.itemObject.getSprite();
            RectTransform rectTransform = fluidObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = spriteSize;
            return fluidObject;
        }

        public string GetToolTip(object obj)
        {
            if (obj is not ItemSlot fluidItem) return null;
            return $"Storing {ItemDisplayUtils.FormatAmountText(fluidItem.amount,false,ItemState.Fluid)} of {fluidItem.itemObject.name}\n";
        }

        public object CreateDeepCopy(object obj)
        {
            return obj is not ItemSlot fluidItem ? null : new ItemSlot(fluidItem.itemObject, fluidItem.amount, null);
        }

        public ItemTagVisualLayer GetLayer()
        {
            return ItemTagVisualLayer.Back;
        }

        public bool AreStackable(object first, object second)
        {
            if (first is not ItemSlot firstSlot) return false;
            if (second is not ItemSlot secondSlot) return false;
            return String.Equals(firstSlot.itemObject?.id, secondSlot.itemObject?.id);
        }
    }
}

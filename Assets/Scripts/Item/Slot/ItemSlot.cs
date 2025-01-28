using Items;
using Items.Tags;
using Items.Transmutable;
using UnityEngine;

namespace Item.Slot
{
    public enum ItemState {
        Solid,
        Fluid
    }

    public static class ItemStateExtension {
        public static Color getSlotColor(this ItemState itemState) {
            switch (itemState) {
                case ItemState.Solid:
                    return ItemDisplayUtils.SolidItemPanelColor;
                case ItemState.Fluid:
                    return ItemDisplayUtils.FluidItemPanelColor;
            }
            return ItemDisplayUtils.SolidItemPanelColor;
        }
    }
    public interface SolidItem {

    }
    public interface NonSolidItem {

    }
    [System.Serializable]
    public class ItemSlot
    {
        public ItemSlot(ItemObject itemObject, uint amount, ItemTagCollection tags) {
            this.itemObject = itemObject;
            this.amount = amount;
            this.tags = tags;
        }
        public ItemObject itemObject;
        public uint amount;
        public ItemTagCollection tags;
        public ItemState getState() {
            switch (itemObject)
            {
                case SolidItem:
                    return ItemState.Solid;
                case TransmutableItemObject transmutableItemObject:
                {
                    TransmutableItemState state = transmutableItemObject.getState();
                    return state.getMatterState();
                }
                case CraftingItem craftingItem:
                    return craftingItem.getItemState();
                case NonSolidItem:
                    return ItemState.Fluid;
                case FluidTileItem:
                    return ItemState.Fluid;
                default:
                    Debug.LogWarning("Get state did not handle state for itemobject " + itemObject.name);
                    return ItemState.Solid;
            }
        }

    }
}
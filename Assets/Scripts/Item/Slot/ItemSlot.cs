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
    
    public interface ISolidItem {

    }
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
        public ItemState GetState() {
            switch (itemObject)
            {
                case ISolidItem:
                    return ItemState.Solid;
                case TransmutableItemObject transmutableItemObject:
                {
                    TransmutableItemState state = transmutableItemObject.getState();
                    return state.getMatterState();
                }
                case CraftingItem craftingItem:
                    return craftingItem.getItemState();
                case FluidTileItem:
                    return ItemState.Fluid;
                default:
                    Debug.LogWarning("Get state did not handle state for itemobject " + itemObject.name);
                    return ItemState.Solid;
            }
        }

    }
}
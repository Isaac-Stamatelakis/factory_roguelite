using Item.GameStage;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Item.Transmutation.Items
{
    public enum TransmutableFluidItemState
    {
        Liquid = TransmutableItemState.Liquid,
        Gas = TransmutableItemState.Gas,
        Plasma = TransmutableItemState.Plasma,
    }
    public class TransmutableFluidTileItemObject : FluidTileItem, ITransmutableItem
    {
        public TransmutableItemMaterial material;
        public TransmutableFluidItemState state;
        public TransmutableItemMaterial getMaterial()
        {
            return material;
        }

        public TransmutableItemState getState()
        {
            return (TransmutableItemState)state;
        }

        public void setState(TransmutableItemState state)
        {
            this.state = (TransmutableFluidItemState)state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            this.material = material;
        }
    }
}

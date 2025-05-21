using Item.GameStage;
using Items;
using Items.Transmutable;
using UnityEngine;

namespace Item.Transmutation.Items
{
    public class TransmutableTileItemObject : TileItem, ITransmutableItem
    {
        public TransmutableItemMaterial material;
        public TransmutableItemState state;

        public TransmutableItemMaterial getMaterial()
        {
            return material;
        }

        public TransmutableItemState getState()
        {
            return state;
        }

        public void setState(TransmutableItemState state)
        {
            this.state = state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            this.material = material;
        }
    }
}

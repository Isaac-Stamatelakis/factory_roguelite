using Items.Transmutable;
using UnityEngine;

namespace Item.Transmutation
{
    public enum TransmutableTileItemState
    {
        Block = TransmutableItemState.Block,
        Brick = TransmutableItemState.Brick,
        Brick_Wall = TransmutableItemState.Brick_Wall,
        Wall = TransmutableItemState.Wall,
    }
    public class TransmutableTileItem : TileItem, ITransmutableItem
    {
        [SerializeField] private TransmutableTileItemState transmutableState;
        [SerializeField] private TransmutableItemMaterial transmutableMaterial;
        public TransmutableItemMaterial getMaterial()
        {
            return transmutableMaterial;
        }

        public TransmutableItemState getState()
        {
            return (TransmutableItemState)transmutableState;
        }

        public void setState(TransmutableItemState state)
        {
            transmutableState = (TransmutableTileItemState)state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            transmutableMaterial = material;
        }
    }
}

using System;
using Items.Transmutable;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Item.Transmutation
{
    public enum TransmutableTileItemState
    {
        Block = TransmutableItemState.Block,
        Brick = TransmutableItemState.Brick,
        Brick_Wall = TransmutableItemState.Brick_Wall,
        Wall = TransmutableItemState.Wall,
        Crystal = TransmutableItemState.Crystal
    }

    public static class TransmutableTileItemStateExtension
    {
        public static TileType GetTileType(this TransmutableTileItemState state)
        {
            switch (state)
            {
                case TransmutableTileItemState.Block:
                case TransmutableTileItemState.Brick:
                    return TileType.Block;
                case TransmutableTileItemState.Brick_Wall:
                case TransmutableTileItemState.Wall:
                    return TileType.Background;
                case TransmutableTileItemState.Crystal:
                    return TileType.Object;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
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

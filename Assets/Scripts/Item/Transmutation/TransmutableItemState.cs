using Item.Slot;

namespace Item.Transmutation
{
    public enum TransmutableItemState {
        Ingot = 0,
        Dust = 1,
        Plate = 2,
        Wire = 3,
        Block = 4,
        Fine_Wire = 5,
        Double_Plate = 6,
        Small_Dust = 7,
        Tiny_Dust = 8,
        Rod = 9,
        Bolt = 10,
        Screw = 11,
        Liquid = 12,
        Gas = 13,
        Plasma = 14,
        Magnificent_Gem = 15,
        Exceptional_Gem = 16,
        Gem = 17,
        Mediocre_Gem = 18,
        Poor_Gem = 19,
        Ore = 20,
        Brick = 21,
        Brick_Wall = 22,
        Wall = 23,
        Crystal = 24,
        Shard = 25
    }
    

    public static class TransmutableItemStateExtension {
        /// <summary>
        /// Returns the ratio of the state in terms of ingot.
        /// <summary>
        public static float GetRatio(this TransmutableItemState state) {
            switch (state) {
                case TransmutableItemState.Ingot:
                    return 1f;
                case TransmutableItemState.Dust:
                    return 1f;
                case TransmutableItemState.Plate:
                    return 1f;
                case TransmutableItemState.Wire:
                    return 2f;
                case TransmutableItemState.Block:
                    return 1/32f;
                case TransmutableItemState.Fine_Wire:
                    return 8f;
                case TransmutableItemState.Double_Plate:
                    return 1/2f;
                case TransmutableItemState.Small_Dust:
                    return 16f;
                case TransmutableItemState.Tiny_Dust:
                    return 4f;
                case TransmutableItemState.Rod:
                    return 2f;
                case TransmutableItemState.Bolt:
                    return 8f;
                case TransmutableItemState.Screw:
                    return 8f;
                case TransmutableItemState.Liquid:
                    return 200f;
                case TransmutableItemState.Gas:
                    return 200f;
                case TransmutableItemState.Plasma:
                    return 200f;
                case TransmutableItemState.Magnificent_Gem:
                    return 1/16f;
                case TransmutableItemState.Exceptional_Gem:
                    return 1/4f;
                case TransmutableItemState.Gem:
                    return 1f;
                case TransmutableItemState.Mediocre_Gem:
                    return 4f;
                case TransmutableItemState.Poor_Gem:
                    return 16f;
                case TransmutableItemState.Ore:
                    return 1 / 2f;
                default:
                    return 0;
            }
        }
        public static ItemState GetMatterState(this TransmutableItemState state) {
            switch (state) {
                case TransmutableItemState.Liquid:
                    return ItemState.Fluid;
                case TransmutableItemState.Gas:
                    return ItemState.Fluid;
                case TransmutableItemState.Plasma:
                    return ItemState.Fluid;
                default:
                    return ItemState.Solid;
            }
        }
    }
}
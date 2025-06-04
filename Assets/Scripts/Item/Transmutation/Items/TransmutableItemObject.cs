using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Slot;
using Item.Transmutation;
using UnityEngine;

namespace Items.Transmutable {
    public enum TransmutableItemObjectState
    {
        Ingot = TransmutableItemState.Ingot,
        Dust = TransmutableItemState.Dust,
        Plate = TransmutableItemState.Plate,
        Wire = TransmutableItemState.Wire,
        Fine_Wire = TransmutableItemState.Fine_Wire,
        Double_Plate = TransmutableItemState.Double_Plate,
        Small_Dust = TransmutableItemState.Small_Dust,
        Tiny_Dust = TransmutableItemState.Tiny_Dust,
        Rod = TransmutableItemState.Rod,
        Bolt = TransmutableItemState.Bolt,
        Screw = TransmutableItemState.Screw,
        Magnificent_Gem = TransmutableItemState.Magnificent_Gem,
        Exceptional_Gem = TransmutableItemState.Exceptional_Gem,
        Gem = TransmutableItemState.Gem,
        Mediocre_Gem = TransmutableItemState.Mediocre_Gem,
        Poor_Gem = TransmutableItemState.Poor_Gem,
        Ore = TransmutableItemState.Ore,
        Shard = TransmutableItemState.Shard
    }
    public class TransmutableItemObject : ItemObject, ITransmutableItem, IStateItem, IColorableItem
    {
        [SerializeField] private TransmutableItemObjectState state;
        [SerializeField] private TransmutableItemMaterial material;
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
            this.state = (TransmutableItemObjectState)state;
        }

        public void setMaterial(TransmutableItemMaterial material)
        {
            this.material = material;
        }

        public ItemState getItemState()
        {
            return ((TransmutableItemState)state).GetMatterState();
        }
        public override Sprite[] GetSprites()
        {
            // Probably worth caching this
            foreach (var stateOptions in material.MaterialOptions.States)
            {
                if (stateOptions.state == state)
                {
                    return stateOptions.sprites;
                }
            }

            return null;
        }

        public override Sprite GetSprite()
        {
            return GetSprites()[0];
        }

        public override ItemDisplayType? getDisplayType()
        {
            var sprites = GetSprites();
            return sprites?.Length > 1 ? ItemDisplayType.Animated : ItemDisplayType.Single;
        }

        public override GameStageObject GetGameStageObject()
        {
            return material?.gameStageObject;
        }

        public override void SetGameStageObject(GameStageObject gameStageObject)
        {
            // Cannot set :)
        }

        public Color Color => material?.color ?? Color.white;
    }
}


using System.Collections;
using System.Collections.Generic;
using Item.GameStage;
using Item.Slot;
using Item.Transmutation;
using UnityEngine;

namespace Items.Transmutable {
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
            return ((TransmutableItemState)state).getMatterState();
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

